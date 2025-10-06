using Microsoft.AspNetCore.Mvc;
using SnitzCore.Data.Interfaces;
using System.Threading.Tasks;

namespace MVCForum.View_Components
{
    public class TopicPreviewViewComponent : ViewComponent
    {
        private readonly IPost _postService;
        public TopicPreviewViewComponent(IPost postService)
        {
            _postService = postService;
        }
        public async Task<IViewComponentResult> InvokeAsync(int id)
        {
            var topic = await _postService.GetTopicWithRelated(id);
            return await Task.FromResult((IViewComponentResult)View(topic));
        }
    }
}
