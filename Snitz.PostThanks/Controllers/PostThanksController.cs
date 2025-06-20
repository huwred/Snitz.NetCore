using BbCodeFormatter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.Options;
using PostThanks.Models;
using Snitz.PhotoAlbum.Models;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;

namespace Snitz.PostThanks.Controllers
{
    public class PostThanksController : Controller
    {
        private readonly ISnitzConfig _config;
        private readonly PostThanksContext _context;
        private readonly ICodeProcessor _bbCodeProcessor;
        private readonly IMember _memberService;
        private readonly string? _tableprefix;
        private readonly SnitzDbContext _snitzContext;
        private readonly PostThanksRepository thanksRepository;
        
        public PostThanksController(ISnitzConfig config,PostThanksContext dbContext,ICodeProcessor BbCodeProcessor,
            IMember memberService,IOptions<SnitzForums> options,SnitzDbContext snitzContext)
        {
            _config = config;
            _context = dbContext;
            _bbCodeProcessor = BbCodeProcessor;
            _memberService = memberService;
            _tableprefix = options.Value.forumTablePrefix;
            _snitzContext = snitzContext;
            thanksRepository = new PostThanksRepository(_context,_config,_snitzContext,options,memberService);
        }
        //
        // GET: /PostThanks/
        /// <summary>
        /// Displays Icon for Post Thanks
        /// </summary>
        /// <param name="id">Id of the user</param>
        /// <param name="replyid"></param>
        /// <param name="showcount"></param>
        /// <returns></returns>
        public ActionResult Index(int id, int replyid = 0, bool showcount = false, bool showlink = true)
        {
                var vm = new PostThanksViewModel
                {
                    UserId = _memberService.Current().Id,
                    TopicId = id,
                    ReplyId = replyid,
                    Thanked = false,
                    ShowCount = showcount,
                    Showlink = showlink
                };
                vm.Thanked = thanksRepository.IsThanked(id, replyid);
                vm.ThanksCount = thanksRepository.Count(id, replyid);
                //vm.PostAuthor = thanksRepository.IsAuthor(id, replyid);

                return PartialView("_Thanks", vm);

        }



        //
        // GET: /Thank
        /// <summary>
        /// Thanks a postc
        /// </summary>
        /// <param name="id">Id of Topic</param>
        /// <param name="replyid"></param>
        /// <param name="returnUrl">Url of calling page</param>
        /// <returns></returns>
        [Authorize]
        public IActionResult Thank(int id, int replyid/*, string returnUrl*/)
        {
            thanksRepository.AddThanks(id, replyid);

            return Json(new { result = "OK", error = "" });

        }

        public PartialViewResult Members(int id, int replyid)
        {
                var members = thanksRepository.Members(id, replyid);
                return PartialView("_Members", members);

        }

        /// <summary>
        /// Un Thank
        /// </summary>
        /// <param name="replyid"></param>
        /// <param name="returnUrl"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public IActionResult UnThank(int id, int replyid/*, string returnUrl = ""*/)
        {
            thanksRepository.DeleteThanks(id, replyid);
            return Json(new { result = "OK", error = "" });

        }

        #region Admin
        //public PartialViewResult ForumThanks(int id)
        //{
        //    ViewBag.ForumId = id;
        //    ViewBag.IsAllowed = thanksRepository.IsForumAllowed(id);
        //    return PartialView("_ForumSetting");
        //}

        //public IActionResult ForumSettings(int id, string check)
        //{
        //    try
        //    {
        //        thanksRepository.SetAllowThanks(id, check=="true"?1:0);
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Error = "Problem updating F_ALLOWTHANKS in Forum";
        //        ViewBag.ErrTitle = "Error: Thanks";
        //        return View("_Error");
        //    }

        //    return new EmptyResult();
        //}
        #endregion

        #region Profile
        public PartialViewResult ThanksProfile(int id)
        {
            var vm = new PostThanksProfile
            {
                Received = thanksRepository.MemberCountReceived(id),
                Given = thanksRepository.MemberCountGiven(id)
            };


            return PartialView("_Profile", vm);

        }

        #endregion

    }
}
