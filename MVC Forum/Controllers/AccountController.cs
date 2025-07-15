using CreativeMinds.StopForumSpam;
using CreativeMinds.StopForumSpam.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MVCForum.ViewModels;
using MVCForum.ViewModels.Member;
using MVCForum.ViewModels.User;
using SmartBreadcrumbs.Nodes;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Data.ViewModels;
using SnitzCore.Service;
using SnitzCore.Service.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using X.PagedList;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace MVCForum.Controllers
{

    public class AccountController : SnitzBaseController
    {
        private readonly UserManager<ForumUser> _userManager;
        private readonly SignInManager<ForumUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly Dictionary<int, MemberRanking>? _ranking;
        private readonly ISnitzCookie _cookie;
        private readonly IWebHostEnvironment _env;
        private readonly int _pageSize;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IOptions<IdentityOptions> _configuration;
        private readonly HttpContext _httpcontext;

        public AccountController(SnitzCore.Data.IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            UserManager<ForumUser> usrMgr, SignInManager<ForumUser> signinMgr,
            ISnitzCookie snitzcookie,IWebHostEnvironment env,IEmailSender mailSender,
            RoleManager<IdentityRole> roleManager,IOptions<IdentityOptions> configuration) : base(memberService, config, localizerFactory, dbContext,httpContextAccessor)
        {
            _userManager = usrMgr;
            _signInManager = signinMgr;
            _ranking = memberService.GetRankings();
            _cookie = snitzcookie;
            _pageSize = 20;
            _emailSender = mailSender;
            _env = env;
            _roleManager = roleManager;
            _configuration = configuration;
            _httpcontext = httpContextAccessor!.HttpContext!;
        }

        [Route("captchacheck/{id?}")]
        public bool CaptchaCheck(int? id)
        {
            if (id != null)
            {
                var session = _httpcontext.Session;

                if (session.Keys.Contains("Captcha") && session.GetString("Captcha") != id.Value.ToString())
                {
                    return false;
                }
                //empty the captcha variable
                session.Remove("Captcha");
                return true;
            }

            return false;
        }

        public IActionResult Index(int pagesize,string? sortOrder,string? sortCol,string? initial, int page=1)
        {

            if (pagesize == 0)
            {
                if (_pageSize > 0)
                {
                    pagesize = _pageSize;
                }
            }
            if(sortOrder == null)
            {
                sortOrder = "asc";
            }
            var admin = User.IsInRole("Administrator");
            var totalCount = _memberService.GetAll(admin).Count();
            IPagedList<Member?> memberListingModel = !string.IsNullOrWhiteSpace(initial) ? _memberService.GetByInitial($"{initial}",out totalCount) : _memberService.GetPagedMembers(admin, pagesize, page,sortCol,sortOrder);
            var pageCount = (int)Math.Ceiling((double)totalCount / pagesize);

            var memberPage = new MvcBreadcrumbNode("Index", "Account", "lblMembers");
            ViewData["BreadcrumbNode"] = memberPage;            

            var members = memberListingModel.Select(m => new MemberListingModel
            {
                Member = m!,
                Id = m!.Id,
                Title = MemberRankTitle(m)!,
                MemberSince = m.Created.FromForumDateStr(),
                LastPost =
                    !string.IsNullOrEmpty(m.Lastpostdate)
                        ? m.Lastpostdate.FromForumDateStr()
                        : null,
                LastLogin = !string.IsNullOrEmpty(m.LastLogin)
                    ? m.LastLogin.FromForumDateStr()
                    : null,
            });
            
            var model = new MemberIndexModel
            {
                PageCount = pageCount,
                PageNum = page,
                PageSize = pagesize,
                MemberList = members
            };
            return View(model);
        }
        public IActionResult ZapMember(int id)
        {
            bool result = _memberService.ZapMember(id);
            
            return Json(new { result = result, data = id });
        }
        public IActionResult Delete(int id)
        {
            var todelete = _memberService.GetById(id);
            if(todelete == null)
            {
                return Json(new { result = false, data = id });
            }
            _memberService.Delete(todelete);
            return Json(new { result = true, data = id });
            
        }
        public async Task<IActionResult> Detail(string? id)
        {
            ForumUser? currUser =  null;
            ForumUser? user;
            Member? member;
            if (User.Identity?.Name != null)
            {
                currUser = (await _userManager.FindByNameAsync(User.Identity?.Name!))!;
            }
            if (id != null)
            {
                member = _memberService.GetByUsername(id);
                user = await _userManager.FindByNameAsync(id);
            }
            else
            {
                user = currUser;
                member = _memberService.GetByUsername(user?.UserName!);
                if (member != null)
                {
                    member!.HideOnline = User.IsInRole("HiddenMembers") ? 1 : 0;
                }
            }
            var memberPage = new MvcBreadcrumbNode("Index", "Account", "lblMembers");
            var myPage = new MvcBreadcrumbNode("Index", "Account", "lblProfile"){ Parent = memberPage };
            ViewData["BreadcrumbNode"] = myPage;  
            try
            {
                var model = new MemberDetailModel
                {
                    Id = member.Id,
                    UserModel = user!, 
                    Name = id ?? member.Name,
                    Firstname = member.Firstname,
                    Lastname = member.Lastname,
                    Title = member.Title,
                    Email = user?.Email ?? member?.Email ?? "",
                    Newemail = user?.Email ?? member?.Email ?? "",
                    Member = member,
                    CanEdit = currUser?.MemberId == member?.Id 
                };
                return View(model);
            }
            catch (Exception e)
            {
                return View("Error");
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult Update(Member model)
        {
            if (model.Dob != null)
            {
                model.Dob = Regex.Replace(model.Dob, "([0-9]{2})/([0-9]{2})/([0-9]{4})", "$3$2$1");
                ModelState.ClearValidationState("Dob");
                ModelState.MarkFieldValid("Dob");
            }

            if (ModelState.IsValid)
            {
                _memberService.Update(model);
                var user = _userManager.FindByNameAsync(model.Name).Result;
                if(user != null)
                {
                    if(model.HideOnline == 1)
                    { 
                        if(_userManager.IsInRoleAsync(user, "HiddenMembers").Result == false)
                        {
                            _userManager.AddToRoleAsync(user, "HiddenMembers");
                        }
                    }
                    else
                    {
                        if(_userManager.IsInRoleAsync(user, "HiddenMembers").Result == true)
                        {
                            _userManager.RemoveFromRoleAsync(user,"HiddenMembers");
                        }
                    }
                }
                 
                return RedirectToAction("Detail", "Account");
            }
            var mdmodel = new MemberDetailModel
            {
                Id = model.Id,
                UserModel = _userManager.FindByNameAsync(model.Name).Result!,
                Name = model.Name,
                Firstname = model.Firstname,
                Lastname = model.Lastname,
                Title = model.Title,
                Email = model!.Email!,
                Newemail = model.Email,
                Member = model,
            };
            return View("Detail", mdmodel);
        }

        [AllowAnonymous]
        [CustomAuthorize(RegCheck = "STRPROHIBITNEWMEMBERS")]
        public ViewResult Register()
        {
            UserCreateModel user = new UserCreateModel
            {
                RequiredFields = _config.GetRequiredMemberFields().ToList()
            };

            return View(user);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(RegCheck = "STRPROHIBITNEWMEMBERS")]
        public async Task<IActionResult> Register(UserCreateModel user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            if (!IsValidEmail(user.Email))
            {
                ModelState.AddModelError("Email","Email not allowed");
                return View(user);
            }
            var ipaddress = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            if (!StopForumSpamCheck(user.Email,user.Name,ipaddress))
            {
                return Redirect("https://www.stopforumspam.com/");
            }
            Member forumMember = new()
            {
                Email = user.Email, 
                Name = user.Name, 
                Level = 1, 
                Status = 0,
                Created = DateTime.UtcNow.ToForumDateStr(),
                Ip = ipaddress,
            };
            var required = new List<KeyValuePair<string, object>>();

            if (user.RequiredFields != null)
            {
                for (int i = 0; i < user.RequiredFields.Count; i++)
                {
                    if (user.RequiredProperty != null)
                        required.Add(new KeyValuePair<string, object>(user.RequiredProperty[i],
                            user.RequiredFields[i]));
                }
            }

            var newmember = _memberService.Create(forumMember, required);
            ForumUser appUser = new()
            {
                UserName = user.Name,
                Email = user.Email,
                MemberId = newmember.Id,
                MemberSince = DateTime.UtcNow,
                LockoutEnabled = true,
                LockoutEnd = DateTime.UtcNow.AddMonths(2),
                EmailConfirmed = false,
                
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
                _emailSender.ParseTemplate("confirmEmail.html",_languageResource["Confirm"].Value,user.Email,user.Name, confirmationLink!, cultureInfo.Name));
            
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
            var returnUrl = login.ReturnUrl ?? Url.Content($"~/");

            ForumUser? newIdentityUser;
            _logger.Info($"Finding User {login.Username}");
            if (!IsValidEmail(login.Username))
            {
                _logger.Info("Finding User by Name");
                newIdentityUser = await _userManager.FindByNameAsync(login.Username);
            }
            else
            {
                try
                {
                    _logger.Info("Finding User by Email");
                    newIdentityUser = await _userManager.FindByEmailAsync(login.Username);
                }
                catch (Exception e)
                {
                    _logger.Error($"Multiple accounts with that email {login.Username}", e);
                    //we will get an error if multiple accounds have the same email;
                    ModelState.AddModelError(nameof(login.Username), "Multiple accounts with that email, please login with your username");
                    return View(login);
                }

            }


            if (newIdentityUser != null) //Already Migrated
            {
                _logger.Info($"Found {newIdentityUser.Email}");
                await _signInManager.SignOutAsync();
                SignInResult result = await _signInManager.PasswordSignInAsync(newIdentityUser, login.Password, login.RememberMe, true);
                if (result.Succeeded)
                {
                    var currmember = _memberService.GetByUsername(login.Username);
                    if (currmember != null)
                    {
                        currmember.LastLogin = DateTime.UtcNow.ToForumDateStr();
                        currmember.LastIp = Request.HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
                        _memberService.Update(currmember);
                    }
                    //_logger.Warn("ReturnUrl1:" + returnUrl);
                    if(Url.IsLocalUrl(returnUrl))
                    {
                        _logger.Info("ReturnUrl is local");
                        return LocalRedirect(returnUrl);
                    }
                    else
                    {
                        _logger.Info("ReturnUrl is not local, redirecting to forum url");
                        returnUrl = _config.ForumUrl;
                    }
                    return LocalRedirect("~/");
                }
                if (result.IsLockedOut)
                {
                    _logger.Info("User is Locked out");
                    var forgotPassLink = Url.Action(nameof(ForgotPassword), "Account", new { }, Request.Scheme);
                    var content =
                        $"Your account is locked out, to reset your password, please click this link: {forgotPassLink}";
                    var message = new EmailMessage(new[] { newIdentityUser.Email! }, "Locked out account information", content);
                    await _emailSender.SendEmailAsync(message);

                    ModelState.AddModelError(nameof(login.Username), "The account is locked out");
                    return View(login);
                }
                ModelState.AddModelError(nameof(login.Username), "Invalid Login Attempt");
                return View();
            }
            _logger.Info("No IdentityUser user, checking Member table");
            var member = _memberService.GetByUsername(login.Username);
            if (member == null)
            {
                //not a member so redirect to the register page
                _logger.Info("Not a member so redirect to the register page");
                return RedirectToActionPermanent("Register");
            }

            _logger.Info("Member found, migrate account");
            return await MigrateMember(login,member, returnUrl);
            
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
                _emailSender.ParseTemplate("forgotPassword.html",_languageResource["Confirm"].Value,user.Email!,user.UserName!,callbackUrl!, cultureInfo.Name));
            
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
        public IActionResult ResetPassword(string? token, string? email)
        {
            if (token == null)
            {
                var forumuser = _userManager.GetUserAsync(_httpcontext.User).Result;
                if (forumuser != null)
                {
                    token = _userManager.GeneratePasswordResetTokenAsync(forumuser).Result;
                    email = forumuser.UserName;
                }
            }

            var model = new ResetPasswordModel { Token = token, Username = email! };
            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(resetPasswordModel);
            var passwordValidator = new PasswordValidator<ForumUser>();
            var result = await passwordValidator.ValidateAsync(_userManager, new ForumUser(), resetPasswordModel.Password);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("Password", "Password is not valid");
                return View(resetPasswordModel);
            }
            _logger.Info($"Reset password for {resetPasswordModel.Username}");
            var user = await _userManager.FindByNameAsync(resetPasswordModel.Username!);
            _logger.Info($"User found={user?.Member?.Name}");

            if (user == null)
                RedirectToAction(nameof(Error));

            _logger.Info($"Reset request {resetPasswordModel.Password} {resetPasswordModel.Token}");
            var resetPassResult = await _userManager.ResetPasswordAsync(user!, resetPasswordModel.Token!, resetPasswordModel.Password!);
            _logger.Info($"Reset result: {resetPassResult.Succeeded}");
            if(!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                    _logger.Error($"{error.Code}:{error.Description}");
                }
                _logger.Warn("Reset errors:");

                return View();
            }
            if (user!.LockoutEnabled)
            {
                _logger.Warn("Disable lockout");
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
        public IActionResult SetTheme(string? theme)
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
                IEnumerable<SnitzCore.Data.Models.Member> listingModel = memberListingModel.ToList()!;
                var totalCount = listingModel.Count();
                var members = listingModel.Select(m => new MemberListingModel
                {
                    Member = m,
                    Id = m.Id,
                    Title = MemberRankTitle(m),
                    MemberSince = m.Created.FromForumDateStr(),
                    LastPost =
                        !string.IsNullOrEmpty(m.Lastpostdate)
                            ? m.Lastpostdate.FromForumDateStr()
                            : null,
                    LastLogin = !string.IsNullOrEmpty(m.LastLogin)
                        ? m.LastLogin.FromForumDateStr()
                        : null,
                });
                var pageCount = (int)Math.Ceiling((double)totalCount / _pageSize);
                var model = new MemberIndexModel
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
                // Remove the lockout date if registration is not restricted 
                if(_config.GetIntValue("STRRESTRICTREG",1) != 1)
                {
                    await _userManager.SetLockoutEndDateAsync(user,null);
                }
                
                var currmember = _memberService.GetByUsername(username);
                if (currmember != null)
                {
                    currmember.LastLogin = DateTime.UtcNow.ToForumDateStr();
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
        public IActionResult NewEmail()
        {
            var member = _memberService.Current();
            var model = new ChangeEmailModel
            {
                Email = "",
                Username = member!.Name,
                CurrentEmail = member!.Email!
            };
            return View(model);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NewEmail(ChangeEmailModel model)
        {
            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null)
            {
                ModelState.AddModelError("CustomError", _languageResource.GetString("UsernameNotFound"));
                return View(model);
            }
            if (!await _userManager.CheckPasswordAsync(user, model!.Password!))
            {
                ModelState.AddModelError("CustomError", _languageResource.GetString("dlgPasswordErr"));                    
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var member = _memberService.Current();
            member!.Newemail = model.NewEmail;
            _snitzDbContext.Update(member);
            await _snitzDbContext.SaveChangesAsync();
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            var token = await _userManager.GenerateChangeEmailTokenAsync(user,model.NewEmail!);//.GenerateEmailConfirmationTokenAsync(user);
            token = HttpUtility.UrlEncode(token);
            var confirmationLink = Url.Action(nameof(ChangeEmail), "Account", new { token, username = member.Name }, Request.Scheme);
            var message = new EmailMessage(new[] { model.NewEmail! }, 
                _languageResource["Confirm"].Value, 
                _emailSender.ParseTemplate("changeEmail.html",_languageResource["Confirm"].Value,model.NewEmail!,member.Name, confirmationLink!, cultureInfo.Name));
            
            await _emailSender.SendEmailAsync(message);
            ViewBag.Message = _languageResource.GetString("EmailConfirm");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeEmail(string? token, string? user)
        {
            if (user == null || token == null)
                return View("Error");            
            
            var forumUser = await _userManager.FindByIdAsync(user);
            var currmember = _memberService.GetById(forumUser?.MemberId);
            if (forumUser == null || currmember == null)
                return View("Error");
            if (currmember.Newemail != null)
            {
                var result = await _userManager.ChangeEmailAsync(forumUser,currmember.Newemail, token);
                if (result.Succeeded)
                {
                    currmember.Email = currmember.Newemail;
                    currmember.Newemail = null;
                    _memberService.Update(currmember);
                    await _snitzDbContext.SaveChangesAsync();
                }

                return View(result.Succeeded ? nameof(ConfirmEmail) : "Error");
            }
            return View("Error");
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

        public IActionResult ShowIP(int id)
        {
            var member = _memberService.GetById(id);
            return Content(member?.LastIp ?? "");
        }

        public async Task<IActionResult> LockMember(int id)
        {
            var member = _memberService.GetById(id);
            if (member != null)
            {
                member.Status = (short)(member.Status == 1 ? 0 : 1);
                _memberService.Update(member);                
                var user = await _userManager.FindByNameAsync(member.Name);
                if (user != null && _userManager.IsInRoleAsync(user, "ForumMember").Result)
                {
                    var res = await _userManager.SetLockoutEnabledAsync(user, true);
                    if (res.Succeeded)
                    {
                        if(member.Status == 1)
                        {
                            await _userManager.SetLockoutEndDateAsync(user,DateTime.MinValue);
                        }
                        else
                        {
                            await _userManager.SetLockoutEndDateAsync(user,DateTime.UtcNow.AddYears(1));
                            await _userManager.UpdateSecurityStampAsync(user);
                        }
                    }
                    return Json(new { result = true, data = id });
                }
            }
            return Json(new { result = true, data = id });
        }

        [HttpGet]
        public IActionResult EmailMember(int? id)
        {
            var member = _memberService.GetById(id);
            if (member?.Email != null)
            {
                var vm = new EmailMemberViewModel
                {
                    To = member.Email
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
                var message = new EmailMessage(new[] { model.To },
                    model.Subject,
                    model.Message);
            
                 _emailSender.SendEmailAsync(message);

                return Json(new { result = true });
            }

            return PartialView(model);

        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangeUsername(int? id)
        {
            if (_memberService.Current()?.Id != id || id == null)
                return View("Error");

            return View(new ChangeUsernameModel{CurrentUserId = _memberService.Current()!.Id});
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult ChangeUsername(ChangeUsernameModel model)
        {
            if (ModelState.IsValid)
            {
                var member = _memberService.GetById(model.CurrentUserId);
                if (_memberService.GetByUsername(model.Username) != null)
                {
                    ModelState.AddModelError("Username",_languageResource.GetString("UserNameExists"));
                    return View(model);
                }
                else
                {
                    member!.Name = model.Username;
                    _memberService.Update(member);
                    _snitzDbContext.SaveChanges();
                    ViewBag.Message = "Username changed";
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteAvatar(int id)
        {
            var currentMember = _memberService.Current();
            if (currentMember != null)
            {
                var avPath = Path.Combine(_env.WebRootPath, _config.ContentFolder,"Avatar");
                try
                {
                    if (System.IO.File.Exists(Path.GetFullPath(avPath,currentMember.PhotoUrl!)))
                    {
                        System.IO.File.Delete(Path.GetFullPath(avPath, currentMember.PhotoUrl!));
                    }
                }
                catch (Exception e)
                {
                    _logger.Error("Unable to delete avatar",e);
                }

                currentMember.PhotoUrl = null;
                _snitzDbContext.Update(currentMember);
                _snitzDbContext.SaveChanges();
            }
            else
            {
                return View("Error");
            }

            return RedirectToAction("Index");
        }
        public IActionResult UploadForm(int? id)
        {
            return PartialView("popUploadAvatar",new ViewModels.UploadViewModel());;
        }

        [Authorize(Roles = "Administrator,Moderator")]
        public PartialViewResult ResolveIP(string ip)
        {
            ViewBag.IPAddress = ip;
            ViewBag.HostName = StringExtensions.GetReverseDNS(ip, 5); //Dns.GetHostEntry(Model.IpAddress).HostName;
            return PartialView("popUserIP");
        }
        public IActionResult UploadAvatar(ViewModels.UploadViewModel model)
        {
            var uploadFolder = Combine(_config.ContentFolder, "Avatar");
            var currentMember = _memberService.Current();
            if (currentMember == null)
            {
                return View("Error");
            }

            if (!Directory.Exists(_env.WebRootPath + uploadFolder))
            {
                Directory.CreateDirectory(_env.WebRootPath + uploadFolder);
            }
            var path = $"{uploadFolder}".Replace("/","\\");
            //return Json(new { result = true, data = Combine(uploadFolder,model.AlbumImage.FileName) });

            if (ModelState.IsValid)
            {
                var uploads = Path.Combine(_env.WebRootPath, path);
                var filePath = Path.Combine(uploads, model.AlbumImage.FileName);
                var fStream = new FileStream(filePath, FileMode.Create);
                model.AlbumImage.CopyTo(fStream);
                fStream.Flush();
                currentMember.PhotoUrl = model.AlbumImage.FileName;
                _snitzDbContext.Update(currentMember);
                _snitzDbContext.SaveChanges();

                return Json(new { result = true, data = "/" + Combine(uploadFolder,model.AlbumImage.FileName) });

            }

            return PartialView("popUploadAvatar",model);
        }

        private bool IsValidEmail(string emailaddress)
        {
            //check spam filter
            if(_config.GetIntValue("STRFILTEREMAILADDRESSES") == 1)
            {
                var spamfilters = _snitzDbContext.SpamFilter.ToList();
                foreach (var spamfilter in spamfilters)
                {
                    if (emailaddress.EndsWith(spamfilter.Server))
                    {
                        Response.Redirect("http://127.0.0.1");
                        return false;
                    }
                }
            }
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
        private string? MemberRankTitle(SnitzCore.Data.Models.Member author)
        {
            var mTitle = author.Title;
            if (author.Status == 0 || author.Name == "n/a")
            {
                mTitle =  _languageResource["tipMemberLocked"].Value;
            }
            if (author.Name == "zapped")
            {
                mTitle =  _languageResource["tipZapped"].Value;
            }

            var rankInfoHelper = new RankInfoHelper(author, ref mTitle, author.Posts, _ranking);
            
            return rankInfoHelper.Title;
        }
        private string GeneratePassword()
        {
            var test = _configuration.Value; // here 
            var opts = new PasswordOptions()
            {
                RequiredLength = test.Password.RequiredLength,
                RequiredUniqueChars = test.Password.RequiredUniqueChars,
                RequireDigit = test.Password.RequireDigit,
                RequireLowercase = test.Password.RequireLowercase,
                RequireNonAlphanumeric = test.Password.RequireNonAlphanumeric,
                RequireUppercase = test.Password.RequireUppercase
            };

            string[] randomChars = new[] {
                "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
                "abcdefghijkmnopqrstuvwxyz",    // lowercase
                "0123456789",                   // digits
                "!@$?_-^"                        // non-alphanumeric
            };

            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();

            if (opts.RequireUppercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[0][rand.Next(0, randomChars[0].Length)]);

            if (opts.RequireLowercase)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[1][rand.Next(0, randomChars[1].Length)]);

            if (opts.RequireDigit)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[2][rand.Next(0, randomChars[2].Length)]);

            if (opts.RequireNonAlphanumeric)
                chars.Insert(rand.Next(0, chars.Count),
                    randomChars[3][rand.Next(0, randomChars[3].Length)]);

            for (int i = chars.Count; i < opts.RequiredLength
                                      || chars.Distinct().Count() < opts.RequiredUniqueChars; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }
        private async Task<IActionResult> MigrateMember(UserSignInModel login, Member member, string returnUrl)
        {
            #region Migrate Member

            var validpwd = false;
            try
            {
                _logger.Info("Validate Old member record");
                validpwd = _memberService.ValidateMember(member!, login.Password);
                _logger.Info("Password is correct.");
                //Password is correct but will it validate, if not then force a reset
                if (validpwd)
                {
                    foreach (var validator in _userManager.PasswordValidators)
                    {
                        var result = await validator.ValidateAsync(_userManager, new ForumUser(), login.Password);

                        if (!result.Succeeded)
                        {
                            _logger.Warn("Password won't validate so forse a reset.");
                            validpwd = false;
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error finding member record", e);
                //membership table may not exists
            }
            if (member != null)
            {
                _logger.Info($"Found Old member record {member.Name}");
                ForumUser existingUser = new()
                {
                    UserName = login.Username,
                    Email = member.Email,
                    MemberId = member.Id,
                    MemberSince = member.Created.FromForumDateStr(),
                    EmailConfirmed = true,
                };
                if (!validpwd)
                {
                    login.Password = GeneratePassword();
                }
                _logger.Info($"Create new Identity user {member.Name} - {login.Password}");
                IdentityResult result = await _userManager.CreateAsync(existingUser, login.Password);
                if (result.Succeeded)
                {
                    var currroles = _snitzDbContext.Set<OldUserInRole>()
                        .Include(u => u.Role).AsNoTracking()
                        .Include(u => u.User).AsNoTracking()
                        .Where(r => r.UserId == member.Id);
                    foreach (var userInRole in currroles)
                    {
                        var exists = _roleManager.Roles.OrderBy(r => r.Name).FirstOrDefault(r => r.Name == userInRole.Role.RoleName);
                        if (exists == null)
                        {
                            await _roleManager.CreateAsync(new IdentityRole(userInRole.Role.RoleName));
                        }
                        await _userManager.AddToRoleAsync(existingUser, userInRole.Role.RoleName);
                    }
                    if (!validpwd)
                    {
                        return LocalRedirect("~/Account/ForgotPassword");
                    }
                    await _signInManager.SignInAsync(existingUser, login.RememberMe);
                    //_logger.Warn("ReturnUrl2:" + returnUrl);
                    if(Url.IsLocalUrl(returnUrl))
                    {
                        _logger.Info("ReturnUrl is local");
                    }
                    else
                    {
                        _logger.Info("ReturnUrl is not local, redirecting to forum url");
                        returnUrl = "~/";
                    }
                    return LocalRedirect(returnUrl);
                }
                foreach (IdentityError error in result.Errors)
                {
                    _logger.Error($"{member.Name} : {error.Description}");
                    ModelState.AddModelError(nameof(login.Username), error.Description);
                }

            }
            else
            {
                _logger.Error($"{login.Username} : Either the user was not found or the password does not match");
                ModelState.AddModelError(nameof(login.Username), "Either the user was not found or the password does not match.<br/>Please try using the forgot password link to reset your password.");
            }
            #endregion

            return View("Login", login);
        }
        private bool StopForumSpamCheck(string email, string name, string? userip)
        {
            Client client = new Client(); //(apiKeyTextBox.Text)
            CreativeMinds.StopForumSpam.Responses.Response response;
            if (!String.IsNullOrWhiteSpace(name) && String.IsNullOrWhiteSpace(email) && String.IsNullOrWhiteSpace(userip))
            {
                response = client.CheckUsername(name);
            }
            else if (String.IsNullOrWhiteSpace(name) && !String.IsNullOrWhiteSpace(email) && String.IsNullOrWhiteSpace(userip))
            {
                response = client.CheckEmailAddress(email);
            }
            else if (String.IsNullOrWhiteSpace(name) && String.IsNullOrWhiteSpace(email) && !String.IsNullOrWhiteSpace(userip))
            {
                response = client.CheckIPAddress(userip);
            }
            else
            {
                response = client.Check(name, email, userip!);
            }
            int freq = 0;
     
            foreach (ResponsePart part in response.ResponseParts)
            {
                freq += part.Frequency;
            }
            return freq < 10;
        }

    }
}
