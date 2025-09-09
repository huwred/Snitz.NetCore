using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MVCForum.ViewModels.Post;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;

namespace MVCForum.View_Components
{
    public class PostControlsViewComponent : ViewComponent
    {
        private readonly IPost _postService;

        public PostControlsViewComponent(IPost postService)
        {
            _postService = postService;
        }
        public async Task<IViewComponentResult> InvokeAsync(dynamic post, bool forlist = false)
        {
            TempData["List"] = forlist;
            if (post is PostReplyModel)
            {
                if(post.Reply == null)
                {
                    return await Task.FromResult((IViewComponentResult)View("Reply",post.ArchivedReply));
                }
                return await Task.FromResult((IViewComponentResult)View("Reply",post.Reply));
            }else if (post is PostReply)
            {
                //var reply = _postService.GetReply(post.Id);
                return await Task.FromResult((IViewComponentResult)View("Reply",post));
            }
            else if (post is Post)
            {
                //var topic = await _postService.GetTopicAsync(post.Id);
                return await Task.FromResult((IViewComponentResult)View("Topic",post));
            }
            else
            {
                //var topic = await _postService.GetTopicAsync(post.Id);
                return await Task.FromResult((IViewComponentResult)View("ArchivedTopic",post));
            }
        }
    }
}
