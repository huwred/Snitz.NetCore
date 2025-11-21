using CreativeMinds.StopForumSpam;
using CreativeMinds.StopForumSpam.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MVCForum.ViewModels;
using MVCForum.ViewModels.Member;
using MVCForum.ViewModels.User;
using NuGet.Common;
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
using static System.Runtime.InteropServices.JavaScript.JSType;
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
        private readonly IPasswordPolicyService _passwordPolicy;

        public AccountController(SnitzCore.Data.IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            UserManager<ForumUser> usrMgr, SignInManager<ForumUser> signinMgr,
            ISnitzCookie snitzcookie,IWebHostEnvironment env,IEmailSender mailSender,
            RoleManager<IdentityRole> roleManager,IOptions<IdentityOptions> configuration,IPasswordPolicyService passwordPolicy) : base(memberService, config, localizerFactory, dbContext,httpContextAccessor)
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
            _passwordPolicy = passwordPolicy;
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
        public IActionResult GetAvatar(string username)
        {
            var member = _memberService.GetByUsername(username);
            if (member != null && !string.IsNullOrWhiteSpace(member.PhotoUrl))
            {
                var avatarPath = Path.Combine(_env.WebRootPath, "Content", "Avatar", member.PhotoUrl);
                if (System.IO.File.Exists(avatarPath))
                {
                    return Content("/Content/Avatar/" + member.PhotoUrl);
                }
            }

            return Content("/images/ninja-1027877_960_720.webp");
        }
        public IActionResult Files(int id)
        {
            ViewBag.MemberId = id;
            return View("Files/Index");
        }
        [CustomAuthorize]
        //[ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any)]
        public IActionResult Index(int pagesize,string? sortdir,string? orderby,string? initial, int page=1)
        {

            if (pagesize == 0)
            {
                if (_pageSize > 0)
                {
                    pagesize = _pageSize;
                }
            }
            if(sortdir == null)
            {
                sortdir = "asc";
            }
            var admin = User.IsInRole("Administrator");
            var totalCount = _memberService.GetAll(admin).Count();
            IPagedList<Member?> memberListingModel = !string.IsNullOrWhiteSpace(initial) ? _memberService.GetByInitial(admin,$"{initial}",out totalCount,pagesize, page,orderby,sortdir) : _memberService.GetPagedMembers(admin, pagesize, page,orderby,sortdir);
            var pageCount = (int)Math.Ceiling((double)totalCount / pagesize);

            var memberPage = new MvcBreadcrumbNode("Index", "Account", "lblMembers");
            ViewData["BreadcrumbNode"] = memberPage;            

            var members = memberListingModel.Select(m => new MemberListingModel
            {
                Member = m!,
                Id = m!.Id,
                Title = MemberRankTitle(m)!,
                Migrated = _userManager.FindByNameAsync(m.Name).Result != null,
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
                MemberList = members,
                SortCol = orderby,
                SortDir = sortdir,
                Initial = initial
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
        public IActionResult DetailPopup(int id, string lang="")
        {
            Member? member = _memberService.GetById(id);
            return PartialView($"Lang/{lang}/popProfile",member);
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
            var myPage = new MvcBreadcrumbNode("Index", "Account", _languageResource["ProfileDetails",member?.Name].Value){ Parent = memberPage };
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
                RequiredFields = _config.GetRequiredMemberFields().ToList(),
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
                Sha256 = 1
            };
            var required = new List<KeyValuePair<string, object>>();

            if (user.RequiredFields != null)
            {
                for (int i = 0; i < user.RequiredFields.Count; i++)
                {
                    if(user.RequiredProperty[i] == "Dob")
                    {
                        //convert date to YYYYMMDD
                        if (DateTime.TryParseExact(user.RequiredFields[i],"dd/mm/yyyy",CultureInfo.InvariantCulture,DateTimeStyles.None, out DateTime dob))
                        {
                            if (dob.Year > DateTime.UtcNow.Year - _config.GetIntValue("STRMINAGE",14))
                            {
                                ModelState.AddModelError("RequiredFields", "Invalid date of birth");
                                return View(user);
                            }
                            else
                            {
                                required.Add(new KeyValuePair<string, object>(user.RequiredProperty[i],
                                    user.RequiredFields[i]));
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("RequiredFields", "Invalid date format");
                            return View(user);
                        }
                    }
                    else
                    if (user.RequiredProperty != null)
                        required.Add(new KeyValuePair<string, object>(user.RequiredProperty[i],
                            user.RequiredFields[i]));
                }
            }
            ForumUser appUser = new()
            {
                UserName = user.Name,
                Email = user.Email,
                MemberSince = DateTime.UtcNow,
                LockoutEnabled = true,
                LockoutEnd = DateTime.UtcNow.AddMonths(2),
                EmailConfirmed = false,
            };            
            
            foreach(IPasswordValidator<ForumUser> passwordValidator in _userManager.PasswordValidators)
            {
                var test = await passwordValidator.ValidateAsync(_userManager, appUser, user.Password);

                if(!test.Succeeded)
                {
                    foreach (IdentityError error in test.Errors)
                        ModelState.AddModelError("Password", error.Description);
                    return View(user);
                }
            }
            //not validating emails so reset fields before creating the accounts
            if (_config.GetIntValue("STREMAILVAL") != 1)
            {
                appUser.LockoutEnabled = false;
                appUser.LockoutEnd = null;
                appUser.EmailConfirmed = true;

                forumMember.Status = 1;
            }
            var newmember = _memberService.Create(forumMember, required);
            appUser.MemberId = newmember.Id;
            IdentityResult result = await _userManager.CreateAsync(appUser, user.Password);
            if (!result.Succeeded)
            {
                _memberService.Delete(newmember);
                foreach (IdentityError error in result.Errors)
                    ModelState.AddModelError("Password", error.Description);
                return View(user);
            }

            CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
            if(_config.GetIntValue("STREMAILVAL") == 1)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                var confirmationLink = Url.Action(nameof(ConfirmEmail), "Account", new { token, username = user.Name }, Request.Scheme);
                var message = new EmailMessage(new[] { user.Email }, 
                    _languageResource["Confirm"].Value, 
                    _emailSender.ParseTemplate("confirmEmail.html",_languageResource["Confirm"].Value,user.Email,user.Name, confirmationLink!, cultureInfo.Name));
            
                await _emailSender.SendEmailAsync(message);
                await _userManager.AddToRoleAsync(appUser, "Visitor");
            }
            if (_config.GetIntValue("STRRESTRICTREG") == 1)
            {
                //don't send email - TODO: need to send email after registration approved!
                return RedirectToAction(nameof(ApproveRegistration));
            }
            if (_config.GetIntValue("STREMAILVAL") == 1 && _config.GetIntValue("STREMAIL") == 1)
            {
                return RedirectToAction(nameof(ConfirmRegEmail));
            }
            //No restrictions so log them in
            //if (user != null)
            //{
            //    WebSecurity.Login(user.UserName, model.Password);
            //    user.LastVisit = DateTime.UtcNow.ToSnitzDate();
            //}

            return RedirectToAction(nameof(SuccessRegistration));
        }

        [HttpGet]
        public IActionResult ApproveRegistration()
        {
            return View();
        }
        [HttpGet]
        public IActionResult SuccessRegistration()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ConfirmRegEmail()
        {
            return View();
        } 
        [HttpGet]
        public IActionResult ConfirmMigrateEmail(string email)
        {
            return View("ConfirmMigrateEmail",email);
        } 
        [AllowAnonymous]
        public IActionResult Login(string returnUrl = "/")
        {
            var remembercookie = _cookie.GetCookieValue("rememberme");

            UserSignInModel login = new()
            {
                ReturnUrl = returnUrl,
                RememberMe = remembercookie == "1"
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
                    TempData["Error"] = "Multiple accounts with that email, please login with your username";
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
                _cookie.SetCookie("rememberme",login.RememberMe ? "1" : "0",DateTime.UtcNow.AddMonths(2));
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
                    TempData["Error"] = "The account is locked out, an email has been sent with details on how to reset your password";
                    ModelState.AddModelError(nameof(login.Username), "The account is locked out");
                    return View(login);
                }
                TempData["Error"] = "Invalid Login Attempt";
                ModelState.AddModelError(nameof(login.Username), "Invalid Login Attempt");
                return View(login);
            }
            _logger.Info("No IdentityUser user, checking Member table");
            var member = _memberService.GetByUsername(login.Username);
            if (member == null)
            {
                //not a member so redirect to the register page
                _logger.Info("Not a member so redirect to the register page");

                return RedirectToActionPermanent("Register");
            }
            if(member.Status != 1)
            {
                _logger.Error($"{login.Username} : Account is locked");
                ModelState.AddModelError(nameof(login.Username), "Your acount is Locked." + Environment.NewLine + "Please contact the Administrator.");
                return View(login);
            }

            _logger.Info("Member found, migrate account");
            ModelState.Clear();
            return await MigrateMember(login,member, returnUrl);
            
        }

        public async Task<IActionResult> Logout(bool clearcookies = false)
        {
            if (clearcookies)
            {
                _cookie.ClearAll();
            }
            else
            {
                _cookie.LogOut();

            }
            await _signInManager.SignOutAsync();
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
            var memberPage = new MvcBreadcrumbNode("Index", "Account", "lblMembers");
            var myPage = new MvcBreadcrumbNode("Detail", "Account", "Profile"){ Parent = memberPage };
            var thisPage = new MvcBreadcrumbNode("ResetPassword", "Account", _languageResource.GetString("PasswordReset")) { Parent = myPage };

            ViewData["BreadcrumbNode"] = thisPage;
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
        [HttpGet]
        [AllowAnonymous]
        public IActionResult MigratePassword(string token, string email)
        {
            var model = new ResetPasswordModel { Token = token, Username = email! };
            ModelState.Clear();
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
            var memberPage = new MvcBreadcrumbNode("Index", "Account", "lblMembers");
            var myPage = new MvcBreadcrumbNode("Detail", "Account", "Profile"){ Parent = memberPage };
            var thisPage = new MvcBreadcrumbNode("NewEmail", "Account", _languageResource.GetString("ChangeEmail")) { Parent = myPage };

            ViewData["BreadcrumbNode"] = thisPage; 
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

            var token = await _userManager.GenerateChangeEmailTokenAsync(user,model.NewEmail!);

            if(!_config.IsEnabled("STREMAILVAL"))
            {
                //if email validation is off then we can go ahead and change the email now
                var result = await _userManager.ChangeEmailAsync(user,model.NewEmail!, token);
                if (result.Succeeded)
                {
                    member.Email = model.NewEmail!;
                    member.Newemail = null;
                    _memberService.Update(member);
                    await _snitzDbContext.SaveChangesAsync();
                    ViewBag.Message = _languageResource.GetString("lblEmailChanged");
                    return View(model);
                }
                ViewBag.Error = "Error changing email";
                return View("Error");
            }
            token = HttpUtility.UrlEncode(token);
            var confirmationLink = Url.Action(nameof(ChangeEmail), "Account", new { token, username = member.Name }, Request.Scheme);
            var message = new EmailMessage(new[] { model.NewEmail! }, 
                _languageResource["Confirm"].Value, 
                _emailSender.ParseTemplate("changeEmail.html",_languageResource["Confirm"].Value,model.NewEmail!,member.Name, confirmationLink!, Thread.CurrentThread.CurrentCulture.Name));
            
            await _emailSender.SendEmailAsync(message);
            ViewBag.Message = _languageResource.GetString("EmailConfirm");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeEmail(string? token, string? username)
        {
            if (username == null || token == null)
            { 
                ViewBag.Error = "Invalid Url parameters";
                return View("Error"); 
            }            
            
            var forumUser = await _userManager.FindByNameAsync(username);
            var currmember = _memberService.GetById(forumUser?.MemberId);
            if (forumUser == null || currmember == null)
            { 
                ViewBag.Error = "User not found";
                return View("Error"); 
            } 
            if (currmember.Newemail != null)
            {
                token = HttpUtility.UrlDecode(token);

                var result = await _userManager.ChangeEmailAsync(forumUser,currmember.Newemail, token);
                if (result.Succeeded)
                {
                    currmember.Email = currmember.Newemail;
                    currmember.Newemail = null;
                    _memberService.Update(currmember);
                    await _snitzDbContext.SaveChangesAsync();
                }

                return View(result.Succeeded ? nameof(ChangeEmail) : "Error");
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
            if(id == -1)
            {
                var adminmember = _config.GetValueWithDefault("STRCONTACTEMAIL",null);
                if (adminmember != null)
                {
                    var vm = new EmailMemberViewModel
                    {
                        To = adminmember
                    };
                    return PartialView(vm);
                }
            }
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
        public IActionResult ContactUs(EmailMemberViewModel model)
        {
            ReturnArgs r = new ReturnArgs();

            if (ModelState.IsValid)
            {
                //Send Email here
                var message = new EmailMessage(new[] { model.To },
                    model.Subject,
                    model.Message);
            
                _emailSender.SendEmailAsync(message);
                r.Status = 200; //good status ... proceed normally
                r.ViewString = _languageResource.GetString("SendEmailSuccess") ;
                return Json(r);

            }

            r.Status = 400; //bad status ... proceed normally
            r.ViewString = this.RenderViewAsync("EmailMember", model, true).Result;
            return Json(r);
        }
        [HttpGet]
        [Authorize]
        public IActionResult ChangeUsername(int? id)
        {
            var memberPage = new MvcBreadcrumbNode("Index", "Account", "lblMembers");
            var myPage = new MvcBreadcrumbNode("Detail", "Account", "Profile"){ Parent = memberPage };
            var thisPage = new MvcBreadcrumbNode("ChangeUsername", "Account", _languageResource.GetString("ChangeUsername")) { Parent = myPage };

            ViewData["BreadcrumbNode"] = thisPage;
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
            if(currentMember != null && currentMember.PhotoUrl != null && currentMember.PhotoUrl.ToLowerInvariant().Contains("http"))
            {
                currentMember.PhotoUrl = null;
                _snitzDbContext.Update(currentMember);
                _snitzDbContext.SaveChanges();
                return RedirectToAction("Index");
            }

            if (currentMember != null && currentMember.PhotoUrl != null)
            {
                var avPath = Path.Combine(_env.WebRootPath, _config.ContentFolder,"Avatar");

                try
                {
                    if (System.IO.File.Exists(Path.Combine(avPath,currentMember.PhotoUrl!)))
                    {
                        System.IO.File.Delete(Path.Combine(avPath, currentMember.PhotoUrl!));
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
            return PartialView("popUpload",new ViewModels.UploadViewModel(){Controller="Account",Action="UploadAvatar",AllowedTypes=".gif,.png,.webp,.jpg"});
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
            var uploadFolder = StringExtensions.UrlCombine(_config.ContentFolder, "Avatar");
            var currentMember = _memberService.Current();
            if (currentMember == null)
            {
                return View("Error");
            }
            if(!IsImage(model.AlbumImage))
            {
                ModelState.AddModelError("AlbumImage", "Invalid image file");
                return PartialView("popUpload",model);
            }

            //var path = $"{uploadFolder}".Replace("/","\\");
            //return Json(new { result = true, data = Combine(uploadFolder,model.AlbumImage.FileName) });

            if (ModelState.IsValid)
            {
                var uploads = Path.Combine(_env.WebRootPath, _config.ContentFolder, "Avatar");
                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }
                var filePath = Path.Combine(uploads, model.AlbumImage.FileName);
                var fStream = new FileStream(filePath, FileMode.Create);
                model.AlbumImage.CopyTo(fStream);
                fStream.Flush();
                currentMember.PhotoUrl = model.AlbumImage.FileName;
                _snitzDbContext.Update(currentMember);
                _snitzDbContext.SaveChanges();

                return Json(new { result = true, data = "/" + StringExtensions.UrlCombine(uploadFolder,model.AlbumImage.FileName) });

            }

            return PartialView("popUpload",model);
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
        private async Task<IActionResult> MigrateMember(UserSignInModel login, Member member, string returnUrl)
        {
            #region Migrate Member

            MigratePassword validpwd = SnitzCore.Data.Models.MigratePassword.InvalidPassword;
            try
            {
                _logger.Info("Validate Old member record");
                validpwd = _memberService.ValidateMember(member, login.Password);
                //Password is correct but will it validate, if not then force a reset
                if (validpwd == SnitzCore.Data.Models.MigratePassword.Valid)
                {
                    _logger.Info("Password is correct.");
                    foreach (var validator in _userManager.PasswordValidators)
                    {
                        var result = await validator.ValidateAsync(_userManager, new ForumUser(), login.Password);

                        if (!result.Succeeded)
                        {
                            _logger.Warn("Password won't validate so force a reset.");
                            validpwd = SnitzCore.Data.Models.MigratePassword.NewPassword;
                            break;
                        }
                    }
                }
                if(validpwd == SnitzCore.Data.Models.MigratePassword.NoMember ) {
                    _logger.Error("Error finding member record");
                }
            }
            catch (Exception e)
            {
                _logger.Error("Error finding member record", e);
                validpwd = SnitzCore.Data.Models.MigratePassword.NoMember;
                //membership table may not exists
            }
            if(validpwd == SnitzCore.Data.Models.MigratePassword.NoMember) {
                ModelState.AddModelError(nameof(login.Username), "No member to migrate");
                return View("Login", login);
            }

            if (validpwd != SnitzCore.Data.Models.MigratePassword.InvalidPassword)
            {
                _logger.Info($"Found Old member record {member.Name}");
                ForumUser newUser = new()
                {
                    UserName = login.Username,
                    Email = member.Email,
                    MemberId = member.Id,
                    MemberSince = member.Created.FromForumDateStr(),
                    EmailConfirmed = true,
                };
                if (validpwd == SnitzCore.Data.Models.MigratePassword.NewPassword)
                {
                    login.Password = _passwordPolicy.GeneratePassword();
                }

                _logger.Info($"Create new Identity user {member.Name} - {login.Password}");
                IdentityResult result = await _userManager.CreateAsync(newUser, login.Password);
                if (result.Succeeded)
                {
                    try //OldUserInRole may not exist so try/catch
                    {
                        var currroles = _snitzDbContext.Set<OldUserInRole>()
                            .Include(u => u.Role).AsNoTracking()
                            .Include(u => u.User).AsNoTracking()
                            .Where(r => r.UserId == member.Id).ToList();
                        if(currroles == null || currroles.Count < 1)
                        {
                            if(member.Level == 3)
                            {
                                await _userManager.AddToRoleAsync(newUser, "Administrator");
                            }
                            if(member.Level == 2)
                            {
                                await _userManager.AddToRoleAsync(newUser, "Moderator");
                            }
                        }
                        else
                        {
                            foreach (var userInRole in currroles)
                            {
                                var exists = _roleManager.Roles.AsNoTracking().OrderBy(r => r.Name).FirstOrDefault(r => r.Name == userInRole.Role.RoleName);
                                if (exists == null)
                                {
                                    await _roleManager.CreateAsync(new IdentityRole(userInRole.Role.RoleName));
                                }
                                await _userManager.AddToRoleAsync(newUser, userInRole.Role.RoleName);
                            }
                        }
                    }
                    catch (Exception)
                    {
                            if(member.Level == 3)
                            {
                                await _userManager.AddToRoleAsync(newUser, "Administrator");
                            }
                            if(member.Level == 2)
                            {
                                await _userManager.AddToRoleAsync(newUser, "Moderator");
                            }

                    }


                    if (validpwd == SnitzCore.Data.Models.MigratePassword.NewPassword)
                    {
                        var token = _userManager.GeneratePasswordResetTokenAsync(newUser).Result;
                        TempData["token"] = token;
                        TempData["username"] = newUser.UserName;
                        var email = newUser.UserName;
                        return RedirectToAction("MigratePassword",new {token,email});
                    }
                    await _signInManager.SignInAsync(newUser, login.RememberMe);

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
            else if (member != null && validpwd == SnitzCore.Data.Models.MigratePassword.InvalidPassword)
            {
                //we need to reset their password, so send a validation email
                ForumUser newUser = new()
                {
                    UserName = login.Username,
                    Email = member.Email,
                    MemberId = member.Id,
                    MemberSince = member.Created.FromForumDateStr(),
                    LockoutEnabled = true,
                    LockoutEnd = DateTime.UtcNow.AddMonths(2),
                    EmailConfirmed = false,
                };
                IdentityResult result = await _userManager.CreateAsync(newUser, _passwordPolicy.GeneratePassword());
                if (result.Succeeded) {
                    CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
                    var confirmationLink = Url.Action(nameof(MigrateConfirmEmail), "Account", new { token, username = newUser.UserName }, Request.Scheme);
                    var message = new EmailMessage(new[] { newUser.Email }, 
                        _languageResource["Confirm"].Value, 
                        _emailSender.ParseTemplate("confirmEmail.html",_languageResource["Confirm"].Value,newUser.Email,newUser.UserName, confirmationLink!, cultureInfo.Name));
                    await _userManager.AddToRoleAsync(newUser, "Visitor");

                    Task.Run(async () => await _emailSender.SendEmailAsync(message));

                    return RedirectToAction(nameof(ConfirmMigrateEmail),new{email = member.Email});
                }
                string commaSeparated = string.Join(", ", result.Errors.Select(r => string.Join(" ", r.Description)));
                _logger.Error($"{member.Name} : {commaSeparated}");
                ModelState.AddModelError(nameof(login.Username), commaSeparated);
            }
            else
            {
                _logger.Error($"{login.Username} : Either the user was not found or the password does not match");
                ModelState.AddModelError(nameof(login.Username), "Either the user was not found or the password does not match. Please try using the forgot password link to reset your password.");
            }
            #endregion

            return View("Login", login);
        }

        public async Task<IActionResult> MigrateConfirmEmail(string token, string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return View("Error");
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                // Remove the lockout date if registration is not restricted 
                await _userManager.SetLockoutEndDateAsync(user,null);
                
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

                var resettoken = _userManager.GeneratePasswordResetTokenAsync(user).Result;
                TempData["token"] = resettoken;
                TempData["username"] = username;

                return RedirectToAction("MigratePassword",new {token=resettoken,email=username});
            }
            string commaSeparated = string.Join(", ", result.Errors.Select(r => string.Join(" ", r.Description)));
            ViewBag.Error = commaSeparated;
            return View("Error");        
        }

        private bool StopForumSpamCheck(string email, string name, string? userip)
        {
            int freq = 0;
            try
            {
                Client client = new Client(); //(apiKeyTextBox.Text)
                CreativeMinds.StopForumSpam.Responses.Response response;
                if (!string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(userip))
                {
                    response = client.CheckUsername(name);
                }
                else if (string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(userip))
                {
                    response = client.CheckEmailAddress(email);
                }
                else if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(userip))
                {
                    response = client.CheckIPAddress(userip);
                }
                else
                {
                    response = client.Check(name, email, userip!);
                }
                foreach (ResponsePart part in response.ResponseParts)
                {
                    freq += part.Frequency;
                }
            }
            catch (Exception)
            {
                _logger.Warn("Error connecting to StopForumSpam");
            }

            return freq < 10;
        }

    }
    public class ReturnArgs
    {
        public ReturnArgs()
        {
        }

        public int Status { get; set; }
        public string ViewString { get; set; }
    }
}
