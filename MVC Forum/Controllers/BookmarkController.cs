using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using MVCForum.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;

namespace MVCForum.Controllers
{
    public class BookmarkController : SnitzController
    {
        private readonly IBookmark _bookmarks;
        public BookmarkController(IMember memberService, ISnitzConfig config, IHtmlLocalizerFactory localizerFactory,SnitzDbContext dbContext,IHttpContextAccessor httpContextAccessor,
            IBookmark bookmarks) : base(memberService, config, localizerFactory, dbContext, httpContextAccessor)
        {
            _bookmarks = bookmarks;
        }

        // GET: BookmarkController
        public ActionResult Index()
        {
            var vm = new BookmarkViewModel
            {
                Bookmarks = _bookmarks.GetAll(),
                ActiveSince = ActiveSince.LastVisit,
                Refresh = ActiveRefresh.None
            };

            return View("Index",vm);
        }


        // POST: BookmarkController/Create
        [HttpPost]
        public ActionResult Create(int id)
        {
            _bookmarks.AddBookMark(id);
            return Json(new { result = "OK", error = "" });
        }

        // POST: BookmarkController/Delete/5
        [HttpPost]
        public ActionResult Delete(int id)
        {
            _bookmarks.DeleteBookMark(id);
            return Json(new { result = "OK", error = "" });
        }


    }
}
