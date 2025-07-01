using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System;
using SnitzCore.Data.Models;


namespace MVCForum.ViewModels
{
    public class SplitTopicViewModel
    {
        public int Id { get; set; }
        public SnitzCore.Data.Models.Post? Topic { get; set; }
        public IEnumerable<PostReply> Replies { get; set; }

        public Dictionary<int, string>? ForumList { get; set; }

        [Range(1,Int32.MaxValue,ErrorMessage ="Please select a Forum")]
        public int ForumId { get; set; }
        
        [Required(ErrorMessage = "Please provide a subject")]
        public string Subject { get; set; }

        public SplitTopicViewModel()
        {
            this.ForumList = new Dictionary<int, string> { { 0, "Select Forum" } };
            this.Replies = new HashSet<PostReply>();
            this.Subject = string.Empty;
        }
    }
}
