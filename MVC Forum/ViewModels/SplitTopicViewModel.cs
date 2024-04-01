using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using System;
using System.Linq;
using SnitzCore.Data;
using SnitzCore.Data.Models;
using log4net.Layout.Members;
using SnitzCore.Data.Interfaces;
using SnitzCore.Service;
using SnitzCore.Data.Interfaces;

namespace MVCForum.ViewModels
{
    public class SplitTopicViewModel
    {
        public int Id { get; set; }
        public SnitzCore.Data.Models.Post? Topic { get; set; }
        public IEnumerable<PostReply>? Replies { get; set; }

        public Dictionary<int, string>? ForumList { get; set; }

        [Required]
        [Range(0, Int32.MaxValue,ErrorMessage = "You must select a Forum")]
        public int ForumId { get; set; }
        [Required]
        public string Subject { get; set; }

        private readonly IForum _forumService;
        public SplitTopicViewModel(IForum forumService)
        {
            this.ForumList = new Dictionary<int, string> { { -1, "Select Forum" } };
            this._forumService = forumService;
            foreach (KeyValuePair<int, string> forum in forumService.ForumList())
            {
                if(!this.ForumList.ContainsKey(forum.Key))
                    this.ForumList.Add(forum.Key, forum.Value);
            }
        }
        //public SplitTopicViewModel(IPrincipal user)
        //{
            
        //    foreach (KeyValuePair<int, string> forum in _forumService.ForumList())
        //    {
        //        if(!this.ForumList.ContainsKey(forum.Key))
        //            this.ForumList.Add(forum.Key, forum.Value);
        //    }
        //}

    }
}
