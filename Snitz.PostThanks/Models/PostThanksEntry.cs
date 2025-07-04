﻿using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snitz.PostThanks.Models
{
    [SnitzTable("THANKS","FORUM")]
    public class PostThanksEntry
    {

        [Column("TOPIC_ID")]
        public int TopicId { get; set; }

        [Column("MEMBER_ID")]
        [ForeignKey("Member")]
        public int MemberId { get; set; }

        [Column("REPLY_ID")]
        public int ReplyId { get; set; }

        public virtual Member? Member { get; set; }
    }

    public class UserThanksCount
    {
        public int TopicCount { get; set; }
        public int ReplyCount { get; set; }
    }
}
