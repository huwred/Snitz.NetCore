using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SnitzCore.BackOffice.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;

namespace SnitzCore.BackOffice.Controllers
{
    [Authorize(Roles="Admin")]
    public class SnitzConfigController : Controller
    {
        private readonly ISnitzConfig _config;
        private readonly SnitzDbContext _context;
        public SnitzConfigController(ISnitzConfig config,SnitzDbContext dbContext)
        {
            _config = config;
            _context = dbContext;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SaveFeatures(IFormCollection form)
        {
            return PartialView("SaveResult");
        }

        public IActionResult SaveBadword(IFormCollection form)
        {
            return PartialView("SaveResult");
        }

        public IActionResult SaveUsername()
        {
            return PartialView("SaveResult");
        }

        public IActionResult RankingConfig()
        {
            var vm = new RankingViewModel(_context,_config);
            return PartialView("ManageRanking",vm);
        }
    }
}
