﻿using Microsoft.AspNetCore.Mvc;
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
        public async Task<IViewComponentResult> InvokeAsync(int postid, Post? thispost = null)
        {
            if(thispost != null) {
                return await Task.FromResult((IViewComponentResult)View(thispost));
            }else {
                var post = _postService.GetTopicAsync(postid).Result;

                return await Task.FromResult((IViewComponentResult)View(post));
            }

        }
    }
}
