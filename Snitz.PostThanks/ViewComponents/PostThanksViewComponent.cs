using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PostThanks.Models;
using Snitz.PhotoAlbum.Models;
using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;

namespace Snitz.PostThanks.ViewComponents
{
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

        public async Task<IViewComponentResult> InvokeAsync(string template,int? id = 0,int? topicid = 0, bool showcount = false)
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
                    Showlink = true
                };
                vm.Thanked = thanksRepository.IsThanked(topicid.Value, id.Value);
                vm.ThanksCount = thanksRepository.Count(topicid.Value, id.Value);
                vm.PostAuthor = thanksRepository.IsAuthor(topicid.Value, id.Value);
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
