﻿using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SnitzCore.BackOffice.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using static SnitzCore.BackOffice.ViewModels.AdminModeratorsViewModel;

namespace SnitzCore.BackOffice.Controllers
{
    [Authorize(Roles="Administrator")]
    public class AdminController : Controller
    {
        private readonly ISnitz _config;
        private readonly IForum _forumservice;
        private readonly ICategory _categoryservice;
        private readonly SnitzDbContext _dbcontext;
        private readonly UserManager<ForumUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ISnitzConfig _snitzconfig;
        private readonly IConfiguration _webconfiguration;
        private readonly IMember _memberService;
        private readonly IOptionsSnapshot<EmailConfiguration> _emailconfig;
        private readonly IWebHostEnvironment _env;
        private IHostApplicationLifetime _appLifetime;
        public AdminController(ISnitz config,IConfiguration configuration, ISnitzConfig snitzconfig,IForum forumservice,ICategory category,SnitzDbContext dbContext,RoleManager<IdentityRole> RoleManager,UserManager<ForumUser> userManager,
            IMember memberService,IOptionsSnapshot<EmailConfiguration> emailconfig,IHostApplicationLifetime appLifetime, IWebHostEnvironment env)
        {
            _config = config;
            _forumservice = forumservice;
            _categoryservice = category;
            _dbcontext = dbContext;
            _userManager = userManager;
            _roleManager = RoleManager;
            _snitzconfig = snitzconfig;
            _webconfiguration = configuration;
            _memberService = memberService;
            _emailconfig = emailconfig;
            _appLifetime = appLifetime;
            _env = env;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Setup()
        {
            return View();
        }
        public IActionResult Features()
        {
            return View("FeatureConfig");
        }
        public IActionResult Forum()
        {
            var model = new AdminModeratorsViewModel(_forumservice)
            {
                Groups = _categoryservice.GetGroups().ToList(),
                Badwords = _snitzconfig.GetBadwords().ToList(),
                UserNamefilters = _memberService.UserNameFilter().ToList()
            };
            ViewBag.Moderators = _config.GetForumModerators();
            return View(model);
        }
        public IActionResult Members()
        {
            return View();
        }
        [HttpGet]
        public IActionResult ManageSubscriptions(int id)
        {
            var subs = new List<SubscriptionItemViewModel>();
            switch (id)
            {
                case 0 :
                    subs = _dbcontext.MemberSubscription
                        .AsNoTracking()
                        .Select(subscription => new SubscriptionItemViewModel
                        {
                            SubscriptionId = subscription.Id,
                            MemberName = subscription.Member.Name,
                            CategoryName = subscription.Category.Name ?? "",
                            ForumName = subscription.Forum.Title ?? "",
                            Topic = subscription.Post.Title ?? ""
                        })
                        .ToList();
                    break;
                case 1 :  //board
                    subs = _dbcontext.MemberSubscription
                            .Where(ms=>ms.PostId == 0 && ms.ForumId == 0 && ms.CategoryId == 0)
                            .AsNoTracking()
                            .Select(subscription => new SubscriptionItemViewModel
                            {
                                SubscriptionId = subscription.Id,
                                MemberName = subscription.Member.Name,
                            })

                        .ToList();
                    break;

                case 2: //category
                    subs = _dbcontext.MemberSubscription
                        .Where(ms=>ms.PostId == 0 && ms.ForumId == 0 && ms.CategoryId != 0)
                        .AsNoTracking()
                        .Select(subscription => new SubscriptionItemViewModel
                        {
                            SubscriptionId = subscription.Id,
                            MemberName = subscription.Member.Name,
                            CategoryName = subscription.Category.Name
                        })
                        .ToList();
                    break;

                case 3: //forum
                    subs = _dbcontext.MemberSubscription
                        .Where(ms=>ms.PostId == 0 && ms.ForumId != 0)
                        .AsNoTracking()
                        .Select(subscription => new SubscriptionItemViewModel
                        {
                            SubscriptionId = subscription.Id,
                            MemberName = subscription.Member.Name,
                            CategoryName = subscription.Category.Name,
                            ForumName = subscription.Forum.Title
                        })
                        .ToList();
                    break;

                case 4: //topic
                    subs = _dbcontext.MemberSubscription
                        .Where(ms=>ms.PostId != 0)
                        .AsNoTracking()
                        .Select(subscription => new SubscriptionItemViewModel
                        {
                            SubscriptionId = subscription.Id,
                            MemberName = subscription.Member.Name,
                            CategoryName = subscription.Category.Name,
                            ForumName = subscription.Forum.Title,
                            Topic = subscription.Post.Title
                        })
                        .ToList();
                    break;

            }

            var vm = new SubscriptionsViewModel()
            {
                Filter = id,
                Subscriptions = subs
            };

            return PartialView(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ManageSubscriptions(IFormCollection form)
        {
            var filter = Convert.ToInt32(form["Filter"]);
            switch (form["Filter"])
            {
                case "1" :  //board
                     _dbcontext.MemberSubscription.Where(ms=>ms.PostId == 0 && ms.ForumId == 0 && ms.CategoryId == 0).ExecuteDeleteAsync();
                    break;

                case "2": //category
                     _dbcontext.MemberSubscription.Where(ms=>ms.PostId == 0 && ms.ForumId == 0 && ms.CategoryId != 0).ExecuteDeleteAsync();
                    break;

                case "3": //forum
                     _dbcontext.MemberSubscription.Where(ms=>ms.PostId == 0 && ms.ForumId != 0).ExecuteDeleteAsync();
                    break;

                case "4": //topic
                     _dbcontext.MemberSubscription.Where(ms=>ms.PostId != 0).ExecuteDeleteAsync();
                    break;

            }
            return RedirectToAction("ManageSubscriptions",new{id=filter});

        }
        public ActionResult ManageGroups(int id = 0)
        {
            AdminGroupsViewModel vm = new AdminGroupsViewModel(id,_categoryservice)
            {
                Groups = _categoryservice.GetGroupNames().ToList(),
            };
            if (id > 0)
            {

            }
            return View(vm);
        }
        public ActionResult GetForumModerators(int id)
        {
            Forum forum = _forumservice.GetById(id);
            AdminModeratorsViewModel vm = new(_forumservice) {ForumId = id};
            Dictionary<int, string?>? modList = forum.ForumModerators?.ToDictionary(o => o.MemberId, o => o.Member?.Name);

            ViewBag.Moderators = _config.GetForumModerators();
            ViewBag.Moderation = forum.Moderation;
            if (modList != null)
            {
                foreach (var (key, _) in modList)
                {
                    vm.ForumModerators.Add(key);
                }
            }

            return PartialView("_Moderators",vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateForumModerators(AdminModeratorsViewModel model)
        {
            Forum forum = _forumservice.GetById(model.ForumId);
            var forumModerators = model.ForumModerators;
            var currentForumModerators = forum.ForumModerators?.ToList();

            if (currentForumModerators != null && !currentForumModerators.Any() )
            {
                //No current moderators so add any new ones
                if (forumModerators.Any())
                {
                    foreach (var moderator in forumModerators)
                    {
                        _dbcontext.ForumModerator.Add(new ForumModerator() { ForumId = model.ForumId, MemberId = moderator });
                    }

                    _dbcontext.SaveChanges();
                }
            }
            else
            {
                if (currentForumModerators != null)
                {
                    foreach (var moderator in currentForumModerators)
                    {
                        //remove any moderators not in new list
                        if (!forumModerators.Contains(moderator.MemberId))
                        {
                            _dbcontext.ForumModerator.Attach(moderator);
                            _dbcontext.Remove(moderator);
                        }
                    }

                    _dbcontext.SaveChanges();
                }

                //refresh list of Forum moderators
                currentForumModerators = forum.ForumModerators?.ToList();
                foreach (var memberid in forumModerators)
                {
                    //Add any new moderators
                    if (currentForumModerators != null)
                    {
                        var exists = currentForumModerators.Find(f => f.MemberId == memberid);
                        if (exists == null)
                        {
                            _dbcontext.ForumModerator.Add(new ForumModerator() { ForumId = model.ForumId, MemberId = memberid });

                        }
                    }
                }
            }
            if (forum.ForumModerators != null)
            {
                foreach (var (key, _) in forum.ForumModerators?.ToDictionary(o => o.MemberId, o => o.Member!.Name)!)
                {
                    model.ForumModerators.Add(key);
                }
            }
            return PartialView("_Moderators",model);
        }

        [Authorize]
        public JsonResult AutoCompleteUsername(string term)
        {

            IEnumerable<string?> result = _userManager.Users.Where(r => r.NormalizedUserName!.Contains(term.ToUpper())).Select(r=>r.UserName);
            return Json(result);
        }
        public async Task<IActionResult> ManageRoles(string role = "")
        {
            List<string> names = new List<string>();
            var vm = new AdminRolesViewModel
                {
                    RoleList = _dbcontext.Roles.Select(r => r.Name).ToArray(), 
                    Rolename = role ?? ""
                };
            if (role != "")
            {
                IdentityRole? Role = await _roleManager.FindByNameAsync(role!);
                if (Role != null)
                {
                    foreach (var user in _userManager.Users)
                    {
                        if (_userManager.IsInRoleAsync(user, Role.Name!).Result)
                            names.Add(user.UserName!);
                    }
                }

                vm.Members = (from s in _dbcontext.Members
                    where names.Contains(s.Name)
                    select s).ToList();
            }
            return PartialView("ManageRoles",vm);
        }
        public async Task<IActionResult> AddMemberToRole(AdminRolesViewModel model)
        {
            if (model.Username != null)
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null)
                {
                    if (model.Rolename != null)
                    {
                        var result = await _userManager.AddToRoleAsync(user, model.Rolename);
                        if (!result.Succeeded)
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError(model.Username,error.Description);
                            }
                    
                        }
                    }
                }
            }
            List<string> names = new List<string>();

            model.RoleList = _dbcontext.Roles.Select(r => r.Name).ToArray();
            if (!string.IsNullOrWhiteSpace(model.Rolename))
            {
                IdentityRole? Role = await _roleManager.FindByNameAsync(model.Rolename);
                if (Role != null)
                {
                    foreach (var user in _userManager.Users)
                    {
                        if (_userManager.IsInRoleAsync(user, Role.Name!).Result)
                            names.Add(user.UserName!);
                    }
                }

                model.Members = (from s in _dbcontext.Members
                    where names.Contains(s.Name)
                    select s).ToList();
            }
            return PartialView("ManageRoles",model);
        }
        public async Task<IActionResult> AddRole(AdminRolesViewModel model)
        {
            if (model.NewRolename != null)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(model.NewRolename));
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(model.NewRolename,error.Description);
                    }
                }
                model.Rolename = model.NewRolename;
            }
            List<string> names = new List<string>();

            model.RoleList = _dbcontext.Roles.Select(r => r.Name).ToArray();
            if (model.NewRolename != null)
            {
                IdentityRole? Role = await _roleManager.FindByNameAsync(model.NewRolename);
                if (Role != null)
                {
                    foreach (var user in _userManager.Users)
                    {
                        if (_userManager.IsInRoleAsync(user, Role.Name!).Result)
                            names.Add(user.UserName!);
                    }
                }
            }

            model.Members = (from s in _dbcontext.Members
                where names.Contains(s.Name)
                select s).ToList();

            return PartialView("ManageRoles",model);
        }
        public async Task<IActionResult> DeleteRole(string rolename)
        {
            var Role = await _roleManager.FindByNameAsync(rolename);
            if (Role != null)
            {
                var result = await _roleManager.DeleteAsync(Role);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(rolename,error.Description);
                    }
                }
            }

            List<string> names = new List<string>();
            var vm = new AdminRolesViewModel
            {
                RoleList = _dbcontext.Roles.Select(r => r.Name).ToArray(), 
                Rolename = rolename ?? ""
            };
            if (rolename != "")
            {
                if (Role != null)
                {
                    foreach (var user in _userManager.Users)
                    {
                        if (_userManager.IsInRoleAsync(user, Role.Name!).Result)
                            names.Add(user.UserName!);
                    }
                }

                vm.Members = (from s in _dbcontext.Members
                    where names.Contains(s.Name)
                    select s).ToList();
            }
            //return Json(new { result = true });
            return PartialView("ManageRoles",vm);
        }

        public async Task<IActionResult> DelMemberFromRole(string username, string role)
        {
            var forumuser = await _userManager.FindByNameAsync(username);
            await _userManager.RemoveFromRoleAsync(forumuser, role);
            List<string> names = new List<string>();
            var vm = new AdminRolesViewModel
            {
                RoleList = _dbcontext.Roles.Select(r => r.Name).ToArray(), 
                Rolename = role ?? ""
            };
            foreach (var user in _userManager.Users)
            {
                if (_userManager.IsInRoleAsync(user, role!).Result)
                    names.Add(user.UserName!);
            }

            vm.Members = (from s in _dbcontext.Members
                where names.Contains(s.Name)
                select s).ToList();
            return PartialView("ManageRoles",vm);
        }

        public IActionResult ArchiveSettings()
        {
            var vm = new ArchivesViewModel() { Categories = _dbcontext.Categories.Include(c=>c.Forums).ToList() };
            return PartialView("ManageArchives", vm);

        }

        public IActionResult ArchiveForum(int id)
        {
            throw new NotImplementedException();
        }

        public IActionResult EmailConfigUpdate(AdminEmailServer model)
        {
            return PartialView("SaveResult");
        }
        public IActionResult EmailConfig()
        {
            AdminEmailServer vm = new AdminEmailServer();
            

            var mailSettings =
                _webconfiguration["MailSettings"];

            vm.Port = _emailconfig.Value.Port;// Convert.ToInt32(mailSettings.GetChildren().Single(s=>s.Key == "Port").Value);
            vm.Server = _emailconfig.Value.SmtpServer;// mailSettings.GetChildren().Single(s=>s.Key == "SmtpServer").Value;
            vm.Password = _emailconfig.Value.Password;// mailSettings.GetChildren().Single(s=>s.Key == "Password").Value;
            vm.Username = _emailconfig.Value.UserName;// mailSettings.GetChildren().Single(s=>s.Key == "UserName").Value;
            vm.From = _emailconfig.Value.From!;// mailSettings.GetChildren().Single(s=>s.Key == "From").Value;
            vm.SslMode = _emailconfig.Value.SecureSocketOptions;// mailSettings.GetChildren().Single(s => s.Key == "SecureSocketOptions").Value;
            vm.DefaultCred = false;
            if (vm.DefaultCred || !string.IsNullOrEmpty(vm.Username))
                vm.Auth = true;
            //vm.Auth = vm.Auth || mailSettings.GetChildren().Single(s => s.Key == "RequireLogin").Value == "true";
            
            vm.EmailMode = _snitzconfig.GetValue("STREMAIL");
            vm.UseSpamFilter = _snitzconfig.GetValue("STRFILTEREMAILADDRESSES");
            vm.ContactEmail = _snitzconfig.GetValue("STRCONTACTEMAIL",null) ?? vm.From;
            vm.BannedDomains = _dbcontext.SpamFilter.AsQueryable().OrderBy(s=>s.Server).ToArray();
            return View(vm);
        }

        public IActionResult AddSpamDomain(IFormCollection form)
        {
            //did we change the enable flag
            string spamfilter = "STRFILTEREMAILADDRESSES";
            var currval = _snitzconfig.GetIntValue(spamfilter);
            if (currval != Convert.ToInt32(form[spamfilter][0]))
            {
                var conf = _dbcontext.SnitzConfig.SingleOrDefault(c => c.Key == spamfilter);
                if (conf != null)
                {
                    conf.Value = form[spamfilter][0];
                    _dbcontext.SnitzConfig.Update(conf);
                    _dbcontext.SaveChanges();
                    var service = new InMemoryCache() { DoNotExpire = true };
                    service.Remove("cfg_" + spamfilter);

                }
                return PartialView("SaveResult","Filter address flag changed");
                
            }
            
            if (string.IsNullOrWhiteSpace(form["EmailDomain"][0]))
            {
                return PartialView("SaveResult","Please provide a domain to add");
            }
            try
            {
                var spamdomain = _dbcontext.SpamFilter.SingleOrDefault(f => f.Server == form["EmailDomain"][0]);
                if (spamdomain != null)
                {
                    return PartialView("SaveResult","Domain already in list");
                }

                if (form["EmailDomain"][0] != null)
                {
                    _dbcontext.SpamFilter.Add(new SpamFilter() { Server = form["EmailDomain"][0]! });
                    _dbcontext.SaveChanges();
                    return PartialView("SaveResult","Domain added");

                }
            }
            catch (Exception e)
            {
                return PartialView("SaveResult",e.Message);
            }
            return PartialView("SaveResult","Error");
        }

        public IActionResult DeleteSpamFilters()
        {
            throw new NotImplementedException();
        }

        public IActionResult Import()
        {
            throw new NotImplementedException();
        }

        public IActionResult PendingMembers()
        {
            var test = _userManager.Users.Include(u=>u.Member).Where(u => !u.EmailConfirmed && u.LockoutEnabled);

            return PartialView(test);
        }
        public IActionResult SaveSpamDomain(IFormCollection form)
        {
            try
            {
                var prefix = $"BannedDomains[{form["counter"][0]}]";
                var spamdomain = _dbcontext.SpamFilter.Find(Convert.ToInt32(form[$"{prefix}.Id"][0]));
                spamdomain!.Server = form[$"{prefix}.Server"][0]!;
                _dbcontext.SpamFilter.Update(spamdomain);
                _dbcontext.SaveChanges();
                return PartialView("SaveResult","Domain info updated");
            }
            catch (Exception e)
            {
                return PartialView("SaveResult",e.Message);
            }
        }

        public IActionResult Restart()
        {
            _appLifetime.StopApplication();
            return RedirectToAction("Index");
        }
        public IActionResult DeleteSpamDomain(IFormCollection form)
        {
            try
            {
                var prefix = $"BannedDomains[{form["counter"][0]}]";
                var spamdomain = _dbcontext.SpamFilter.Find(Convert.ToInt32(form[$"{prefix}.Id"][0]));
                _dbcontext.Remove(spamdomain!);
                _dbcontext.SaveChanges();

                return PartialView("SaveResult","Domain removed");
            }
            catch (Exception e)
            {
                return PartialView("SaveResult",e.Message);
            }
        }

        public IActionResult ListViews()
        {
            return View();
        }

        public IActionResult LoadFile(string id)
        {
            string contents = System.IO.File.ReadAllText(_env.ContentRootPath + $@"\{id}");

            return Content(contents);
        }
    }
}
