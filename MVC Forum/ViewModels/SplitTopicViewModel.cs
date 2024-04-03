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

        [Range(1,Int32.MaxValue,ErrorMessage ="Please select a Forum")]
        public int ForumId { get; set; }
        
        [Required(ErrorMessage = "Please provide a subject")]
        public string Subject { get; set; }

        public SplitTopicViewModel()
        {
            this.ForumList = new Dictionary<int, string> { { 0, "Select Forum" } };

        }

    }
}
