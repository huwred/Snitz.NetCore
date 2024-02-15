using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using MimeKit;
using MVCForum.Models.Member;
using MVCForum.Models.User;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using X.PagedList;
using Microsoft.AspNetCore.Mvc.Localization;


namespace MVCForum.Controllers
{
    
    public class AccountController : SnitzController
    {
        private readonly UserManager<ForumUser> _userManager;
        private readonly SignInManager<ForumUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly Dictionary<int, MemberRanking>? _ranking;
        private readonly ISnitzCookie _cookie;
        private readonly IWebHostEnvironment _env;
        private readonly int _pageSize;

        public AccountController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            UserManager<ForumUser> usrMgr, SignInManager<ForumUser> signinMgr,
            ISnitzCookie snitzcookie,IWebHostEnvironment env,IEmailSender mailSender) : base(memberService, config, localizerFactory, dbContext,httpContextAccessor)
        {
            _userManager = usrMgr;
            _signInManager = signinMgr;
            _ranking = memberService.GetRankings();
            _cookie = snitzcookie;
            _pageSize = config.DefaultPageSize;
            _emailSender = mailSender;
            _env = env;
        }

        public IActionResult Index(int pagesize,string? sortOrder,string? sortCol,string? initial, int page=1)
        {
            _memberService.SetLastHere(User);
            if (pagesize == 0)
            {
                if (_pageSize > 0)
                {
                    pagesize = _pageSize;
                }
            }
            var totalCount = _memberService.GetAll().Count();
            IPagedList<Member?> memberListingModel = !string.IsNullOrWhiteSpace(initial) ? _memberService.GetByInitial($"{initial}",out totalCount) : _memberService.GetPagedMembers(pagesize, page);
            var pageCount = (int)Math.Ceiling((double)totalCount / pagesize);
            

            var members = memberListingModel.Select(m => new MemberListingModel()
            {
                Member = m!,
                Id = m!.Id,
                Title = MemberRankTitle(m)!,
                MemberSince = m.Created.FromForumDateStr(),
                LastPost =
                    !string.IsNullOrEmpty(m.Lastpostdate)
                        ? m.Lastpostdate.FromForumDateStr()
                        : null,
                LastHereDate = !string.IsNullOrEmpty(m.Lastheredate)
                    ? m.Lastheredate.FromForumDateStr()
                    : null,
            });
            
            var model = new MemberIndexModel()
            {
                PageCount = pageCount,
                PageNum = page,
                PageSize = pagesize,
                MemberList = members
            };
            return View(model);
        }

        public async Task<IActionResult> Detail(string? id)
        {
            ForumUser? currUser =  null;
            if (User.Identity?.Name != null)
            {
                currUser = (await _userManager.FindByNameAsync(User.Identity?.Name!))!;
            }
            ForumUser? user;
            Member? member = null;
            if (id != null)
            {
                member = _memberService.GetByUsername(id);
                user = await _userManager.FindByNameAsync(id);
            }
            else
            {
                //Get current logged in user record because no username passed in
                var memberid = _userManager.GetUserId(User);
                user = await _userManager.FindByIdAsync(memberid!);
                member = _memberService.GetByUsername(user?.UserName!);
            }

            var model = new MemberDetailModel()
            {
                Id = member!.Id,
                UserModel = user!, 
                Username = id ?? member.Name,
                Firstname = member.Firstname,
                Lastname = member.Lastname,
                Title = member.Title,
                Email = user?.Email ?? member.Email,
                Member = member,
                CanEdit = currUser?.UserName == member.Name || _userManager.IsInRoleAsync(currUser!,"Admin").Result
            };

            if (user != null )
                return View(model);
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Update(Member model)
        {
            if (model.Dob != null)
            {
                model.Dob = Regex.Replace(model.Dob, "([0-9]{2})/([0-9]{2})/([0-9]{4})", "$3$2$1");
                ModelState.ClearValidationState("Dob");
                ModelState.MarkFieldValid("Dob");
            }

            if (ModelState.IsValid)
            {
                if (model.Email != model.Newemail)
                {
                    _memberService.Update(model);
                    var currUser =  _userManager.FindByNameAsync(model.Name).Result;
                    
                    //Email has changed send validation email
                    CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                    var token = _userManager.GenerateChangeEmailTokenAsync(currUser!,model.Newemail!).Result;
                    var confirmationLink = Url.Action(nameof(ChangeEmail), "Account", new { token, username = currUser!.UserName }, Request.Scheme);
                    var message = new EmailMessage(new[] { model.Newemail! }, 
                        "Confirmation email link", 
                        ParseTemplate("changeEmail.html","Confirm Account",model.Newemail!,currUser.UserName!, confirmationLink!, cultureInfo));
            
                    await _emailSender.SendEmailAsync(message);
                }
                else
                {
                    _memberService.Update(model);
                }
                
                return RedirectToAction("Detail","Account");
            }
            var mdmodel = new MemberDetailModel()
            {
                Id = model.Id,
                UserModel = _userManager.FindByNameAsync(model.Name).Result!, 
                Username = model.Name,
                Firstname = model.Firstname,
                Lastname = model.Lastname,
                Title = model.Title,
                Email = model.Email,
                Member = model,
            };            
            return View("Detail",mdmodel);
        }

        [AllowAnonymous]
        [CustomAuthorize(RegCheck = "STRPROHIBITNEWMEMBERS")]
        public ViewResult Register()
        {
            UserCreateModel user = new UserCreateModel();
            user.RequiredFields = _config.GetRequiredMemberFields().ToList();
            //Config.GetRequiredMemberFields();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [CustomAuthorize(RegCheck = "STRPROHIBITNEWMEMBERS")]
        public async Task<IActionResult> Register(UserCreateModel user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            Member forumMember = new()
            {
                Email = user.Email, 
                Name = user.Name, 
                Level = 1, 
                Status = 0,
                Created = DateTime.UtcNow.ToForumDateStr()
            };
            var required = new List<KeyValuePair<string, object>>();
            if (user.RequiredFields != null)
                foreach (var requiredField in user.RequiredFields)
                {
                    required.Add(new KeyValuePair<string, object>(requiredField,
                        HttpContext.Request.Form[requiredField][0]!));
                }

            var newmember = _memberService.Create(forumMember, required);
            ForumUser appUser = new()
            {
                UserName = user.Name,
                Email = user.Email,
                MemberId = newmember.Id,
                MemberSince = DateTime.UtcNow,

            };
            IdentityResult result = await _userManager.CreateAsync(appUser, user.Password);
            if (!result.Succeeded)
            {
                _memberService.Delete(newmember);
                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                return View(user);
            }

            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, username = user.Name }, Request.Scheme);
            var message = new EmailMessage(new[] { user.Email }, 
                _languageResource["Confirm"].Value, 
                ParseTemplate("confirmEmail.html",_languageResource["Confirm"].Value,user.Email,user.Name, confirmationLink!, cultureInfo));
            
            await _emailSender.SendEmailAsync(message);
            await _userManager.AddToRoleAsync(appUser, "Visitor");


            return RedirectToAction(nameof(SuccessRegistration));
        }

        [HttpGet]
        public IActionResult SuccessRegistration()
        {
            return View();
        }
 
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = "/")
        {
            UserSignInModel login = new()
            {
                ReturnUrl = returnUrl
            };
            ModelState.Clear();
            return View(login);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserSignInModel login)
        {
            if (!ModelState.IsValid)
            {
                return View(login);
            }
            var returnUrl = login.ReturnUrl ?? Url.Content("~/");

            ForumUser? appUser = null;
            _logger.Warn($"Finding User {login.Username}");
            if (IsValidEmail(login.Username))
            {
                try
                {
                    _logger.Warn($"Finding User by Email");
                    appUser = await _userManager.FindByEmailAsync(login.Username);
                }
                catch (Exception e)
                {
                    _logger.Error($"Multiple accounts with that email {login.Username}",e);
                    //we will get an error if multiple accounds have the same email;
                    ModelState.AddModelError(nameof(login.Username), "Multiple accounts with that email, please login with your username");
                    return View();
                }
                
            }
            else
            {
                _logger.Warn($"Finding User by Name");
                appUser = await _userManager.FindByNameAsync(login.Username);
            }
            

            if (appUser != null)
            {
                _logger.Warn($"Found {appUser.Email}");
                await _signInManager.SignOutAsync();
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(appUser, login.Password, login.RememberMe, true);
                if (result.Succeeded)
                {
                    var currmember = _memberService.GetByUsername(login.Username);
                    if (currmember != null)
                    {
                        currmember.Lastheredate = DateTime.UtcNow.ToForumDateStr();
                        _memberService.Update(currmember);
                    }

                    return LocalRedirect(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    _logger.Warn($"User is Locked out");
                    var forgotPassLink = Url.Action(nameof(ForgotPassword),"Account", new { }, Request.Scheme);
                    var content =
                        $"Your account is locked out, to reset your password, please click this link: {forgotPassLink}";
                    var message = new EmailMessage(new[] { appUser.Email! }, "Locked out account information", content);
                    await _emailSender.SendEmailAsync(message);
                    _logger.Warn($"No IdentityUser userEmai sent");
                    ModelState.AddModelError(nameof(login.Username), "The account is locked out");
                    return View();
                }
                ModelState.AddModelError(nameof(login.Username), "Invalid Login Attempt");
                return View();                       
            }
            _logger.Warn($"No IdentityUser user");
            //No IdentityUser user, so check the member table,
            //if member exists then create IdentityUser.
            var member = _memberService.GetByUsername(login.Username);
            var validpwd = false;
            try
            {
                _logger.Warn($"Find Old member record");
                validpwd = _memberService.ValidateMember(member!, login.Password);
            }
            catch (Exception e)
            { 
                _logger.Error("Find Old member record",e);
                //membership table may not exists
            }
            if (member != null)
            {
                _logger.Warn($"Found Old member record {member?.Name}");
                ForumUser existingUser = new()
                {
                    UserName = login.Username,
                    Email = member!.Email,
                    MemberId = member.Id,
                    MemberSince = member.Created.FromForumDateStr(),
                    EmailConfirmed = true,
                };
                if (!validpwd)
                {
                    login.Password = "S0meR@ndom3Str!ng";
                }
                _logger.Warn($"Create new Identity user {member?.Name}");
                IdentityResult result = await _userManager.CreateAsync(existingUser, login.Password);
                if (result.Succeeded)
                {
                    if (!validpwd)
                    {
                        return LocalRedirect("/Account/ForgotPassword");
                    }
                    await _signInManager.SignInAsync(existingUser, false);
                    return LocalRedirect(returnUrl);
                }
                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError(nameof(login.Username), error.Description);
            }
            else
            {
                ModelState.AddModelError(nameof(login.Username), "Either the user was not found or the password does not match.<br/>Please try using the forgot password link to reset your password.");
            }

            return View(login);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _cookie.LogOut();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(forgotPasswordModel);
            var user = await _userManager.FindByNameAsync(forgotPasswordModel.Username!);
            if (user == null)
            {

                return RedirectToAction(nameof(ForgotPasswordConfirmation));

            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.UserName }, Request.Scheme);
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            var message = new EmailMessage(new[] { user.Email! }, 
                _languageResource["Confirm"].Value, 
                ParseTemplate("forgotPassword.html",_languageResource["Confirm"].Value,user.Email!,user.UserName!,callbackUrl!, cultureInfo));
            
            await _emailSender.SendEmailAsync(message);
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }
        
        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPasswordModel { Token = token, Username = email };
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(resetPasswordModel);
            _logger.Warn($"Reset password for {resetPasswordModel.Username}");
            var user = await _userManager.FindByNameAsync(resetPasswordModel.Username!);
            _logger.Warn($"User found={user?.Member?.Name}");

            if (user == null)
                RedirectToAction(nameof(Error));

            _logger.Warn($"Reset request {resetPasswordModel.Password} {resetPasswordModel.Token}");
            var resetPassResult = await _userManager.ResetPasswordAsync(user!, resetPasswordModel.Token!, resetPasswordModel.Password!);
            _logger.Warn($"Reset result: {resetPassResult.Succeeded}");
            if(!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                    _logger.Warn($"{error.Code}:{error.Description}");
                }
                _logger.Warn($"Reset errors:");

                return View();
            }
            if (user!.LockoutEnabled)
            {
                _logger.Warn($"Disable lockout");
                await _userManager.SetLockoutEnabledAsync(user, false);
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }
        
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        public IActionResult Inbox()
        {
            return RedirectToAction("Index","PrivateMessage");
        }
        
        [AllowAnonymous]
        public IActionResult SetTheme(string theme)
        {
            if (theme == null)
            {
                _cookie.Clear("snitztheme");
            }
            else
            {
                _cookie.SetCookie("snitztheme",theme,DateTime.MaxValue);
            }
            
            return new JsonResult("OK");
        }
        
        [HttpPost]
        public IActionResult Search(IFormCollection form)
        {

            var memberListingModel = _memberService.GetFilteredMembers(form["SearchFor"]!,form["SearchIn"]!);
            if (memberListingModel != null)
            {
                IEnumerable<Member>? listingModel = memberListingModel.ToList()!;
                var totalCount = listingModel.Count();
                var members = listingModel.Select(m => new MemberListingModel()
                {
                    Member = m,
                    Id = m.Id,
                    Title = MemberRankTitle(m),
                    MemberSince = m.Created.FromForumDateStr(),
                    LastPost =
                        !string.IsNullOrEmpty(m.Lastpostdate)
                            ? m.Lastpostdate.FromForumDateStr()
                            : null,
                    LastHereDate = !string.IsNullOrEmpty(m.Lastheredate)
                        ? m.Lastheredate.FromForumDateStr()
                        : null,
                });
                var pageCount = (int)Math.Ceiling((double)totalCount / _pageSize);
                var model = new MemberIndexModel()
                {
                    PageCount = pageCount,
                    PageNum = 1,
                    PageSize = Convert.ToInt32(form["pagesize"]),
                    MemberList = members
                };
                return View("Index", model);
            }

            return RedirectToAction("Index");
        }
        
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string token, string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return View("Error");
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                var currmember = _memberService.GetByUsername(username);
                if (currmember != null)
                {
                    currmember.Lastheredate = DateTime.UtcNow.ToForumDateStr();
                    currmember.Status = 1;
                    _memberService.Update(currmember);
                }

                if (!_userManager.IsInRoleAsync(user,"ForumMember").Result)
                {
                    await _userManager.AddToRoleAsync(user, "ForumMember");
                }
                if (_userManager.IsInRoleAsync(user, "Visitor").Result)
                {
                    await _userManager.RemoveFromRoleAsync(user, "Visitor");
                }
            }
            else
            {
                var currmember = _memberService.GetByUsername(username);
                if (user.Email != currmember?.Newemail)
                {
                    currmember!.Email = currmember.Newemail!;
                }
            }
            return View(result.Succeeded ? nameof(ConfirmEmail) : "Error");
        }
        
        [HttpGet]
        public async Task<IActionResult> ChangeEmail(string token, string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            var currmember = _memberService.GetByUsername(username);
            if (user == null || currmember?.Newemail == null)
                return View("Error");
            var result = await _userManager.ChangeEmailAsync(user,currmember.Newemail, token);
            if (result.Succeeded)
            {
                currmember.Email = currmember.Newemail;
                currmember.Newemail = null;
                _memberService.Update(currmember);
            }

            return View(result.Succeeded ? nameof(ConfirmEmail) : "Error");
        }
        
        [HttpGet]
        public IActionResult Error()
        {
            return View();
        }
        
        public IActionResult TestEmail()
        {
            throw new NotImplementedException();
        }

        private bool IsValidEmail(string emailaddress)
        {
            try
            {
                MailAddress m = new(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        private string? MemberRankTitle(Member author)
        {

            string? mTitle = author.Title;
            if (author.Status == 0 || author.Name == "n/a")
            {
                mTitle =  _languageResource["tipMemberLocked"].Value;
            }
            if (author.Name == "zapped")
            {
                mTitle =  _languageResource["tipZapped"].Value;
            }

            var rankInfoHelper = new RankInfoHelper(author, ref mTitle, author.Posts, _ranking);

            return mTitle;
        }
        private string ParseTemplate(string template,string subject, string email,string username, string callbackUrl, CultureInfo? culture)
        {
            if (culture != null)
            {
                template = culture.Name + Path.DirectorySeparatorChar + template;
            }
            var pathToFile = _env.WebRootPath  
                             + Path.DirectorySeparatorChar  
                             + "Templates"  
                             + Path.DirectorySeparatorChar  
                             + template;
            var builder = new BodyBuilder();
            using (StreamReader sourceReader = System.IO.File.OpenText(pathToFile))
            {

                builder.HtmlBody = sourceReader.ReadToEnd();

            }

            string messageBody = builder.HtmlBody
                .Replace("[SUBJECT]",subject)
                .Replace("[DATE]",$"{DateTime.Now:dddd, d MMMM yyyy}")
                .Replace("[EMAIL]",email)
                .Replace("[USER]",username)
                .Replace("[SERVER]",_config.ForumUrl)
                .Replace("[FORUM]",_config.ForumTitle)
                .Replace("[URL]",callbackUrl);
            return messageBody;
        }

        public IActionResult ShowIP(int id)
        {
            var member = _memberService.GetById(id);
            return Content(member?.LastIp ?? "");
            throw new NotImplementedException();
        }

        public async Task<IActionResult> LockMember(int id)
        {
            var member = _memberService.GetById(id);
            if (member != null)
            {
                var user = await _userManager.FindByNameAsync(member.Name);
                if (user != null && _userManager.IsInRoleAsync(user, "ForumMember").Result)
                {
                    return Json(new { result = true, data = id });
                }
                member.Status = (short)(member.Status == 1 ? 0 : 1);
                _memberService.Update(member);
            }


            return Json(new { result = true, data = id });

        }

        [HttpGet]
        public IActionResult EmailMember(int? id)
        {
            var member = _memberService.GetById(id);
            if (member?.Email != null)
            {
                var vm = new EmailMemberViewModel()
                {
                    To = member?.Email
                };
                return PartialView(vm);
            }

            return View("Error");
        }

        public IActionResult EmailMember(EmailMemberViewModel model)
        {
            if (ModelState.IsValid)
            {
                //Send Email here
                var message = new EmailMessage(new[] { model.To! },
                    model.Subject,
                    model.Message);
            
                 _emailSender.SendEmailAsync(message);

                return Json(new { result = true });
            }
            else
            {
                return PartialView(model);
            }

        }
    }
}
