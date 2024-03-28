using System.Collections.Generic;
using SnitzCore.Data.Models;

namespace MVCForum.ViewModels.Post
{
    public class PollViewModel
    {
        public int TopicId { get; set; }
        public Poll Poll { get; set; } 
        public IEnumerable<PollVote> Votes { get; set; }
    }
}
