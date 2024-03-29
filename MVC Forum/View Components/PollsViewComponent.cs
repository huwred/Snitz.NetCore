using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data;
using SnitzCore.Data.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MVCForum.ViewModels.Post;

namespace MVCForum.View_Components
{
    public class PollsViewComponent : ViewComponent
    {
        private readonly SnitzDbContext _dbContext;
        public PollsViewComponent(SnitzDbContext dbContext)
        {
            _dbContext = dbContext;

        }

        public async Task<IViewComponentResult> InvokeAsync(string template, int? catid, int? forumid, int topicid = 0)
        {
            if (template == "ForumConfig")
            {
                var forum = _dbContext.Forums.Find(forumid);
                return await Task.FromResult((IViewComponentResult)View(template,forum));
            }
            if (template == "AddPoll")
            {
                var poll = new Poll
                {
                    ForumId = forumid.Value,
                    CatId = catid.Value
                };
                return await Task.FromResult((IViewComponentResult)View(template,poll));
            }
            if (template == "DisplayPoll")
            {
                var vm = new PollViewModel()
                {
                    TopicId = topicid,
                    Poll = _dbContext.Polls.Include(p => p.PollAnswers).Include(p=>p.Topic).SingleOrDefault(x => x.TopicId == topicid),
                    Votes = _dbContext.PollVotes.Include(p => p.Poll.PollAnswers).Where(p => p.PostId == topicid)
                };

                return await Task.FromResult((IViewComponentResult)View(template,vm));
            }

            if (template == "Featured")
            {
                var vm = new PollViewModel()
                {
                    Poll = _dbContext.Polls.Include(p => p.PollAnswers).Include(p=>p.Topic).SingleOrDefault(x => x.Id == topicid),
                    Votes = _dbContext.PollVotes.Include(p => p.Poll.PollAnswers).Where(p => p.PollId == topicid)
                };

                return await Task.FromResult((IViewComponentResult)View("DisplayPoll",vm));
            }
            if (template == "PollSummary")
            {
                TempData["featured"] = false;
                TempData["panel"] = "bg-primary";
                var vm = new PollViewModel()
                {
                    TopicId = topicid,
                    Poll = _dbContext.Polls.Include(p=>p.Topic).Include(p => p.PollAnswers).SingleOrDefault(x => x.TopicId == topicid),
                    Votes = _dbContext.PollVotes.Include(p => p.Poll.PollAnswers).Where(p => p.PostId == topicid)
                };
                return await Task.FromResult((IViewComponentResult)View(template,vm));
            }

            if (template == "Config")
            {
                return await Task.FromResult((IViewComponentResult)View("Config"));
            }
            return await Task.FromResult((IViewComponentResult)View("Default"));
        }
    }
}
