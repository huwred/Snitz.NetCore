using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SnitzCore.BackOffice.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using SnitzCore.Service;
using System.Data;
using static Org.BouncyCastle.Math.EC.ECCurve;
using static SnitzCore.BackOffice.ViewModels.AdminModeratorsViewModel;

namespace SnitzCore.BackOffice.Controllers
{
    public class ArchiveController : Controller
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

        public ArchiveController(ISnitz config,IConfiguration configuration, ISnitzConfig snitzconfig,IForum forumservice,ICategory category,SnitzDbContext dbContext,RoleManager<IdentityRole> RoleManager,UserManager<ForumUser> userManager,
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
            var vm = new ArchivesViewModel() { Categories = _dbcontext.Categories.Include(c=>c.Forums).ToList() };
            return PartialView("ManageArchives", vm);
        }


        [HttpPost]
        public IActionResult SaveSettings(ArchivesViewModel model)
        {

            var Categories = _dbcontext.Categories.AsNoTracking().ToList();
            //loop through and save any changes
            try
            {
                _dbcontext.Database.BeginTransaction();
                if (model.Categories.Any())
                {
                    foreach (var category in model.Categories)
                    {
                        if(category.Forums != null)
                        {
                            bool update = false;
                            foreach (var forum in category.Forums)
                            {
                                var origforum = _dbcontext.Forums.First(f=>f.Id == forum.Id);

                                if(forum.ArchiveSched != origforum.ArchiveSched)
                                {
                                    update = true;
                                    origforum.ArchiveSched = forum.ArchiveSched;
                                }
                                if(forum.DeleteSched != origforum.DeleteSched)
                                {
                                    update = true;
                                    origforum.DeleteSched = forum.DeleteSched;
                                }
                                if (update)
                                {
                                    _dbcontext.Update(origforum);
                                }                    
                            }
                            _dbcontext.SaveChanges();
                        }
                    }
                }

            }
            catch (Exception)
            {
                _dbcontext.Database.RollbackTransaction();
            }
            finally
            {
                _dbcontext.Database.CommitTransaction();
            }

            return PartialView("SaveResult", "Settings Updated");

        }
        [HttpGet]
        public IActionResult ArchiveForum(int id)
        {

            ArchiveViewModel vm = new ArchiveViewModel {ForumId = id};
            return PartialView("popArchiveForum", vm);
        }
        [HttpGet]
        public IActionResult DeleteArchiveForum(int id)
        {

            BackgroundJob.Enqueue(() => _forumservice.DeleteArchivedTopics(id));

            return Json(new { redirectToUrl = Url.Action("Forum", "Admin") });
        }
        [HttpPost]
        public IActionResult ArchiveForum(ArchiveViewModel vm)
        {
            var archiveDate = DateTime.UtcNow.AddMonths(-vm.MonthsOlder).ToForumDateStr();
            BackgroundJob.Enqueue(() => _forumservice.ArchiveTopics(vm.ForumId, archiveDate));

            var avm = new ArchivesViewModel() { Categories = _dbcontext.Categories.Include(c=>c.Forums).ToList() };
            return View("ManageArchives", avm);
        }

    }
}
