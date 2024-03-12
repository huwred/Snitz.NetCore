using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MVCForum.ViewModels.Post;
using SnitzCore.Data.Interfaces;

namespace MVCForum.View_Components
{
    public class PostControlsViewComponent : ViewComponent
    {
        private readonly IPost _postService;

        public PostControlsViewComponent(IPost postService)
        {
            _postService = postService;
        }
        public async Task<IViewComponentResult> InvokeAsync(dynamic post)
        {
            if (post is PostReplyModel)
            {
                var reply = _postService.GetReply(post.Id);

                return await Task.FromResult((IViewComponentResult)View("Reply",reply));
            }
            else
            {
                var topic = _postService.GetTopic(post.Id);
                return await Task.FromResult((IViewComponentResult)View("Topic",topic));
            }
        }
    }
}
