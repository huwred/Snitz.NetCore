using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PostThanks.Models;
using Snitz.PostThanks.Models;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;

namespace Snitz.PostThanks.ViewComponents
{
    /// <summary>
    /// Represents a view component for rendering various templates related to the "Post Thanks" feature in the forum.
    /// </summary>
    /// <remarks>This view component supports multiple templates, including forum configuration, topic
    /// summaries, user profiles,  and administrative views. It interacts with the database and other services to
    /// retrieve and display data  relevant to the "Post Thanks" functionality.</remarks>
    public class PostThanksViewComponent : ViewComponent
    {
        private readonly SnitzDbContext _dbContext;
        private readonly ISnitzConfig _config;
        private readonly PostThanksContext _thanksContext;
        private readonly PostThanksRepository thanksRepository;
        private readonly IMember _memberService;

        public PostThanksViewComponent(SnitzDbContext dbContext, ISnitzConfig config,PostThanksContext thanksContext,IOptions<SnitzForums> options,IMember memberService)
        {
            _dbContext = dbContext;
            _config = config;
            _thanksContext = thanksContext;
            thanksRepository = new PostThanksRepository(thanksContext,_config,dbContext,options, memberService);
            _memberService = memberService;
        }

        /// <summary>
        /// Asynchronously invokes the specified view component template with the provided parameters.
        /// </summary>
        /// <remarks>The behavior of this method depends on the specified <paramref name="template">:
        /// <list type="bullet"> <item> <description> "ForumConfig": Retrieves forum configuration details and
        /// determines if thanks are allowed for the specified forum. </description> </item> <item> <description>
        /// "TopicSummary": Generates a summary of thanks for a specific topic and reply, including whether the current
        /// user has thanked the post. </description> </item> <item> <description> "Profile": Displays the count of
        /// thanks received and given by a specific user. </description> </item> <item> <description> "EnableButton":
        /// Checks if the "FORUM_THANKS" table exists in the database. </description> </item> <item> <description>
        /// "MenuItem": Determines whether the "Thanks" menu item should be displayed based on configuration settings.
        /// </description> </item> <item> <description> "Admin": Renders the admin view for managing the component.
        /// </description> </item> If the <paramref name="template"> does not match any of the supported templates, an
        /// empty view is returned.</remarks>
        /// <param name="template">The name of the view component template to render. Supported templates include "ForumConfig",
        /// "TopicSummary", "Profile", "EnableButton", "MenuItem", and "Admin".</param>
        /// <param name="id">An optional identifier used by certain templates, such as "ForumConfig", "TopicSummary", and "Profile".
        /// Defaults to 0 if not provided.</param>
        /// <param name="topicid">An optional topic identifier used by the "TopicSummary" template. Defaults to 0 if not provided.</param>
        /// <param name="showcount">A boolean value indicating whether to display the count of thanks in the "TopicSummary" template. Defaults
        /// to <see langword="false"/>.</param>
        /// <param name="archived">A boolean value indicating whether to consider archived posts in the "TopicSummary" template. Defaults to
        /// <see langword="false"/>.</param>
        /// <param name="showlink">An optional boolean value indicating whether to display a link in the "TopicSummary" template. Defaults to
        /// <see langword="true"/>.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see
        /// cref="IViewComponentResult"/> representing the rendered view component.</returns>
        public async Task<IViewComponentResult> InvokeAsync(string template,int? id = 0,int? topicid = 0, bool showcount = false, bool archived = false, bool? showlink = true)
        {
            if (template == "ForumConfig")
            {
                var forum = _dbContext.Forums
                .FromSqlInterpolated($"SELECT * FROM FORUM_FORUM WHERE F_ALLOWTHANKS=1")
                .SingleOrDefault(f => f.Id == id);
                ViewBag.IsAllowed = forum != null;
                ViewBag.ForumId = id;
                return await Task.FromResult((IViewComponentResult)View(template,forum));
            }
            if (template == "TopicSummary" && topicid != null && id != null)
            {
                var vm = new PostThanksViewModel
                {
                    UserId = _memberService.Current().Id,
                    TopicId = topicid.Value,
                    ReplyId = id.Value,
                    Thanked = false,
                    ShowCount = showcount,
                    Showlink = showlink != null ? showlink.Value : true,
                };
                vm.Thanked = thanksRepository.IsThanked(topicid.Value, id.Value,_memberService.Current()?.Id);
                vm.ThanksCount = thanksRepository.Count(topicid.Value, id.Value);
                vm.PostAuthor = thanksRepository.IsAuthor(topicid.Value, id.Value, archived);
                return await Task.FromResult((IViewComponentResult)View(template,vm));
            }
            if(template == "Profile")
            {
                var vm = new PostThanksProfile
                {
                    Received = thanksRepository.MemberCountReceived(id.Value),
                    Given = thanksRepository.MemberCountGiven(id.Value),
                };

                return await Task.FromResult((IViewComponentResult)View(template,vm));
            }
            if (template == "EnableButton")
            {
                var installed = _config.TableExists("FORUM_THANKS");
                return await Task.FromResult((IViewComponentResult)View(template,installed));
            }
            if (template == "MenuItem")
            {
                var showthanks = _config.TableExists("FORUM_THANKS") && _config.GetIntValue("STRTHANKS") == 1;

                return await Task.FromResult((IViewComponentResult)View(template,showthanks));
            }

            if (template == "Admin")
            {
                return await Task.FromResult((IViewComponentResult)View(template));
            }
            return await Task.FromResult((IViewComponentResult)View());
        }

    }
}
