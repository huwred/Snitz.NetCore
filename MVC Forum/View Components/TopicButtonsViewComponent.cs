using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.Threading.Tasks;

namespace MVCForum.View_Components
{
    public class TopicButtonsViewComponent : ViewComponent
    {
        private readonly IPost _postService;
        public TopicButtonsViewComponent(IPost postService) { 
            _postService = postService;
        }
        public async Task<IViewComponentResult> InvokeAsync(int postid)
        {
            var post = await _postService.GetTopicAsync(postid);

            return await Task.FromResult((IViewComponentResult)View(post));
        }
    }
}
