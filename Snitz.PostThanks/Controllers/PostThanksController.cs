using BbCodeFormatter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PostThanks.Models;
using Snitz.PostThanks.Models;
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

        /// <summary>
        /// Renders a partial view displaying the "Thanks" icon for a specific topic or reply.
        /// </summary>
        /// <param name="id">The unique identifier of the topic to display the "Thanks" section for.</param>
        /// <param name="replyid">The unique identifier of the reply within the topic. Defaults to <see langword="0"/> if not specified.</param>
        /// <param name="showcount">A value indicating whether the count of "Thanks" should be displayed. Defaults to <see langword="false"/>.</param>
        /// <param name="showlink">A value indicating whether the "Thanks" link should be displayed. Defaults to <see langword="true"/>.</param>
        /// <returns>A partial view containing the "Thanks" section for the specified topic or reply.</returns>
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
                vm.Thanked = thanksRepository.IsThanked(id, replyid,_memberService.Current()?.Id);
                vm.ThanksCount = thanksRepository.Count(id, replyid);
                //vm.PostAuthor = thanksRepository.IsAuthor(id, replyid);

                return PartialView("_Thanks", vm);

        }

        /// <summary>
        /// Records a "thank you" action for a specific reply in a discussion thread.
        /// </summary>
        /// <remarks>This action requires authorization. The method adds a "thank you" entry for the
        /// specified reply and returns a JSON object with the result of the operation.</remarks>
        /// <param name="id">The identifier of the discussion thread.</param>
        /// <param name="replyid">The identifier of the reply being thanked.</param>
        /// <returns>A JSON result indicating the operation's success or failure.</returns>
        [Authorize]
        public IActionResult Thank(int id, int replyid /*, string returnUrl*/)
        {
            thanksRepository.AddThanks(id, replyid);

            return Json(new { result = "OK", error = "" });

        }

        /// <summary>
        /// Retrieves a list of members who have "thanked" the post and renders a partial view.
        /// </summary>
        /// <param name="id">The identifier of the primary entity for which members are retrieved.</param>
        /// <param name="replyid">The identifier of the reply or related entity used to filter the members.</param>
        /// <returns>A <see cref="PartialViewResult"/> that renders the "_Members" partial view with the retrieved members.</returns>
        public PartialViewResult Members(int id, int replyid)
        {
                var members = thanksRepository.Members(id, replyid);
                return PartialView("_Members", members);

        }

        /// <summary>
        /// Removes a "thank" reaction associated with a specific reply.
        /// </summary>
        /// <param name="id">The unique identifier of the user who gave the "thank".</param>
        /// <param name="replyid">The unique identifier of the reply from which the "thank" is being removed.</param>
        /// <returns>An <see cref="IActionResult"/> containing a JSON object with the result of the operation. The result will
        /// indicate success or failure.</returns>
        public IActionResult UnThank(int id, int replyid/*, string returnUrl = ""*/)
        {
            thanksRepository.DeleteThanks(id, replyid);
            return Json(new { result = "OK", error = "" });

        }

        #region Admin
        /// <summary>
        /// Updates the "Allow Thanks" setting for a specific forum based on the provided parameters.
        /// </summary>
        /// <remarks>If an error occurs during the update process, the method sets error details in the
        /// <see cref="ViewBag"/>  and returns an error view.</remarks>
        /// <param name="id">The unique identifier of the forum whose setting is being updated.</param>
        /// <param name="check">A string representing the desired state of the "Allow Thanks" setting.  Use "true" to enable the setting or
        /// any other value to disable it.</param>
        /// <returns>An empty result indicating the operation has completed.</returns>
        public IActionResult ForumSettings(int id, string check)
        {
            try
            {
                thanksRepository.SetAllowThanks(id, check == "true" ? 1 : 0);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Problem updating F_ALLOWTHANKS in Forum";
                ViewBag.ErrTitle = "Error: Thanks";
                return View("_Error");
            }

            return new EmptyResult();
        }
        #endregion

        #region Profile
        /// <summary>
        /// Returns a partial view displaying the "Thanks" profile for a specified member.
        /// </summary>
        /// <remarks>The view model includes the count of "Thanks" received and given by the specified
        /// member.</remarks>
        /// <param name="id">The unique identifier of the member whose "Thanks" profile is to be retrieved.</param>
        /// <returns>A <see cref="PartialViewResult"/> containing the "_Profile" partial view and the associated view model.</returns>
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
