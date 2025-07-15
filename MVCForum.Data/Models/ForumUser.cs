using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models
{
    public class ForumUser :  IdentityUser
    {
        [ForeignKey("Member")]
        public int MemberId { get; set; }
        public Member? Member { get; set; }
        public string? UserDescription { get; set; }
        public string? ProfileImageUrl { get; set; }
        public int Rating { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
        public DateTime MemberSince { get; set; }

    }

}
