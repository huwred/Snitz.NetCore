using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SnitzCore.BackOffice.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using static SnitzCore.BackOffice.ViewModels.AdminModeratorsViewModel;

namespace SnitzCore.BackOffice.Controllers
{
    [Authorize(Roles="Admin")]
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
        public AdminController(ISnitz config,IConfiguration configuration, ISnitzConfig snitzconfig,IForum forumservice,ICategory category,SnitzDbContext dbContext,RoleManager<IdentityRole> RoleManager,UserManager<ForumUser> userManager)
        {
            _config = config;
            _forumservice = forumservice;
            _categoryservice = category;
            _dbcontext = dbContext;
            _userManager = userManager;
            _roleManager = RoleManager;
            _snitzconfig = snitzconfig;
            _webconfiguration = configuration;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Features()
        {
            return View("FeatureConfig");
        }
        public IActionResult Forum()
        {
            var model = new AdminModeratorsViewModel(User, _forumservice) { Groups = _categoryservice.GetGroups().ToList() };
            ViewBag.Moderators = _config.GetForumModerators();
            return View(model);
        }
        public IActionResult Members()
        {
            return View();
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
            AdminModeratorsViewModel vm = new(User,_forumservice) {ForumId = id};
            Dictionary<int, string> modList = forum.ForumModerators?.ToDictionary(o => o.MemberId, o => o.Member.Name);

            ViewBag.Moderators = _config.GetForumModerators();
            ViewBag.Moderation = forum.Moderation;
            if (modList != null)
            {
                foreach (KeyValuePair<int, string> mod in modList)
                {
                    vm.ForumModerators.Add(mod.Key);
                }
            }

            return PartialView("_Moderators",vm);
        }
        [Authorize]
        public JsonResult AutoCompleteUsername(string term)
        {

            IEnumerable<string> result = _userManager.Users.Where(r => r.NormalizedUserName.Contains(term.ToUpper())).Select(r=>r.UserName);
            return Json(result);
        }
        public IActionResult ManageRoles(string? role)
        {
            List<string> names = new List<string>();
            var vm = new AdminRolesViewModel
                {
                    RoleList = _dbcontext.Roles.Select(r => r.Name).ToArray(), 
                    Rolename = role ?? ""
                };
            if (role != null)
            {
                IdentityRole Role = _roleManager.FindByNameAsync(role).Result;
                if (role != null)
                {
                    foreach (var user in _userManager.Users)
                    {
                        if (user != null && _userManager.IsInRoleAsync(user, Role.Name).Result)
                            names.Add(user.UserName);
                    }
                }

                vm.Members = (from s in _dbcontext.Members
                    where names.Contains(s.Name)
                    select s).ToList();
            }
            return PartialView("ManageRoles",vm);
        }

        public IActionResult DeleteRole(string rolename)
        {
            throw new NotImplementedException();
        }

        public IActionResult DelMemberFromRole(object user, string rolename)
        {
            throw new NotImplementedException();
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

        public IActionResult EmailConfig()
        {
            AdminEmailServer vm = new AdminEmailServer();
            

            var mailSettings =
                _webconfiguration.GetSection("MailSettings");

            if (mailSettings.GetChildren().Any())
            {
                vm.Port = Convert.ToInt32(mailSettings.GetChildren().Single(s=>s.Key == "Port").Value);
                vm.Server = mailSettings.GetChildren().Single(s=>s.Key == "SmtpServer").Value;
                vm.Password = mailSettings.GetChildren().Single(s=>s.Key == "Password").Value;
                vm.Username = mailSettings.GetChildren().Single(s=>s.Key == "UserName").Value;
                vm.From = mailSettings.GetChildren().Single(s=>s.Key == "From").Value;
                vm.SslMode = mailSettings.GetChildren().Single(s => s.Key == "SecureSocketOptions").Value;
                vm.DefaultCred = false;
                if (vm.DefaultCred || !string.IsNullOrEmpty(vm.Username))
                    vm.Auth = true;
                vm.Auth = vm.Auth || mailSettings.GetChildren().Single(s => s.Key == "RequireLogin").Value == "true";
            }
            vm.EmailMode = _snitzconfig.GetValue("STREMAIL");
            vm.UseSpamFilter = _snitzconfig.GetValue("STRFILTEREMAILADDRESSES");
            vm.ContactEmail = _snitzconfig.GetValue("STRCONTACTEMAIL") ?? vm.From;
            vm.BannedDomains = _dbcontext.SpamFilter.AsQueryable().OrderBy(s=>s.Server).ToArray();
            return View(vm);
        }

        public IActionResult AddSpamDomain(string domain)
        {
            throw new NotImplementedException();
        }

        public IActionResult DeleteSpamFilters()
        {
            throw new NotImplementedException();
        }

        public IActionResult Import()
        {
            throw new NotImplementedException();
        }

    }
}
