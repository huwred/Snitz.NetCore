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


namespace MVCForum.Controllers
{
    
    public class AccountController : Controller
    {
        private const string Templatepath = @"Templates";
        private readonly UserManager<ForumUser> _userManager;
        private readonly SignInManager<ForumUser> _signInManager;
        private readonly IMember _memberService;
        private readonly IEmailSender _emailSender;
        private readonly Dictionary<int,MemberRanking> _ranking;
        private readonly ISnitzCookie _cookie;
        private readonly IWebHostEnvironment _env;
        private readonly int _pageSize;

        public AccountController(UserManager<ForumUser> usrMgr, SignInManager<ForumUser> signinMgr, IMember memberService,SnitzCore.Data.Interfaces.IEmailSender mailSender,
            ISnitzConfig config, ISnitzCookie snitzcookie,IWebHostEnvironment env)
        {
            _userManager = usrMgr;
            _signInManager = signinMgr;
            this._memberService = memberService;
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
            IEnumerable<Member> memberListingModel = !string.IsNullOrWhiteSpace(initial) ? _memberService.GetByInitial($"{initial}",out totalCount) : _memberService.GetPagedMembers(pagesize, page);
            var pageCount = (int)Math.Ceiling((double)totalCount / pagesize);
            

            var members = memberListingModel.Select(m => new MemberListingModel()
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
            
            var model = new MemberIndexModel()
            {
                PageCount = pageCount,
                PageNum = page,
                MemberList = members
            };
            return View(model);
        }

        public async Task<IActionResult> Detail(string id)
        {
            var currUser = User.Identity?.Name;
            ForumUser user;
            var member = _memberService.GetByUsername(id);
            if (id != null)
            {
                user = await _userManager.FindByNameAsync(id);
            }
            else
            {
                var memberid = _userManager.GetUserId(User);
                user = await _userManager.FindByIdAsync(memberid);
                member = _memberService.GetByUsername(user.UserName);
            }

            var model = new MemberDetailModel()
            {
                Id = member.Id,
                UserModel = user, 
                Username = id ?? member.Name,
                Firstname = member.Firstname,
                Lastname = member.Lastname,
                Title = member.Title,
                Email = user?.Email ?? member.Email,
                Member = member,
                CanEdit = currUser == user?.UserName
            };

            if (user != null || member != null)
                return View(model);
            else
                return RedirectToAction("Index");
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
                return RedirectToAction("Detail","Account");
            }
            var mdmodel = new MemberDetailModel()
            {
                Id = model.Id,
                UserModel = _userManager.FindByNameAsync(model.Name).Result, 
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
        [CustomAuthorizeAttribute(RegCheck = "STRPROHIBITNEWMEMBERS")]
        public ViewResult Register() => View();
        [HttpPost]
        [AllowAnonymous]
        [CustomAuthorizeAttribute(RegCheck = "STRPROHIBITNEWMEMBERS")]
        public async Task<IActionResult> Register(UserCreateModel user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }
            Member forumMember = new() { Email = user.Email, Name = user.Name, Level = 1, Status = 0,Created = DateTime.UtcNow.ToForumDateStr()};
                
            var newmember = _memberService.Create(forumMember);
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
            CultureInfo uiCultureInfo = Thread.CurrentThread.CurrentUICulture;
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, username = user.Name }, Request.Scheme);
            var message = new EmailMessage(new string[] { user.Email }, 
                "Confirmation email link", 
                ParseTemplate("confirmEmail.html","Confirm Account",user.Email,confirmationLink, cultureInfo));
            
             _emailSender.SendEmailAsync(message);
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
            if (IsValidEmail(login.Username))
            {
                appUser = await _userManager.FindByEmailAsync(login.Username);
            }
            else
            {
                appUser = await _userManager.FindByNameAsync(login.Username);
            }
            

            if (appUser != null)
            {
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
                    var forgotPassLink = Url.Action(nameof(ForgotPassword),"Account", new { }, Request.Scheme);
                    var content = string.Format("Your account is locked out, to reset your password, please click this link: {0}", forgotPassLink);
                    var message = new EmailMessage(new string[] { appUser.Email }, "Locked out account information", content);
                    await _emailSender.SendEmailAsync(message);
                    ModelState.AddModelError(nameof(login.Username), "The account is locked out");
                    return View();
                }
                ModelState.AddModelError(nameof(login.Username), "Invalid Login Attempt");
                return View();                       
            }
            //No IdentityUser user, so check the member table,
            //if member exists then create IdentityUser.
            var member = _memberService.GetByUsername(login.Username);
            var validpwd = _memberService.ValidateMember(member, login.Password);
            if (member != null && validpwd)
            {

                ForumUser existingUser = new()
                {
                    UserName = login.Username,
                    Email = member.Email,
                    MemberId = member.Id,
                    MemberSince = member.Created.FromForumDateStr(),
                    EmailConfirmed = true,
                };
                IdentityResult result = await _userManager.CreateAsync(existingUser, login.Password);
                if (result.Succeeded)
                {
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
            var user = await _userManager.FindByEmailAsync(forgotPasswordModel.Email);
            if (user == null)
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action(nameof(ResetPassword), "Account", new { token, email = user.Email }, Request.Scheme);
            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            var message = new EmailMessage(new string[] { user.Email }, 
                "Reset password token", 
                ParseTemplate("forgotPassword.html","Confirm Account",user.Email,callbackUrl, cultureInfo));
            
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
            var model = new ResetPasswordModel { Token = token, Email = email };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            if (!ModelState.IsValid)
                return View(resetPasswordModel);
            var user = await _userManager.FindByEmailAsync(resetPasswordModel.Email);
            if (user == null)
                RedirectToAction(nameof(ResetPasswordConfirmation));
            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordModel.Token, resetPasswordModel.Password);
            if(!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return View();
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

            var memberListingModel = _memberService.GetFilteredMembers(form["SearchFor"],form["SearchIn"]);
            if (memberListingModel != null)
            {
                IEnumerable<Member> listingModel = memberListingModel.ToList();
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
        private string MemberRankTitle(Member author)
        {

            string? mTitle = author.Title;
            if (author.Status == 0 || author.Name == "n/a")
            {
                mTitle =  "Member Locked"; //ResourceManager.GetLocalisedString("tipMemberLocked", "Tooltip");// "Member Locked";
            }
            if (author.Name == "zapped")
            {
                mTitle = "Zapped Member"; //ResourceManager.GetLocalisedString("tipZapped", "Tooltip");// "Zapped Member";
            }

            var rankInfoHelper = new RankInfoHelper(author, ref mTitle, author.Posts, _ranking);

            return mTitle;
        }
        private string ParseTemplate(string template,string subject, string email, string callbackUrl, CultureInfo? culture)
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
                .Replace("[URL]",callbackUrl);
            return messageBody;
        }

    }
}
