using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace SnitzCore.Service.TagHelpers
{
    [HtmlTargetElement("member-rank", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class MemberRankTagHelper : TagHelper
    {
        public Member Member { get; set; } = new Member();
        public Dictionary<int, MemberRanking>? Ranking { get; set; }
        public ClaimsPrincipal? User { get; set; }
        private readonly UserManager<ForumUser> _userManager;
        public MemberRankTagHelper(UserManager<ForumUser> usrMgr)
        {
            _userManager = usrMgr;

        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "div";
            output.TagMode = TagMode.StartTagAndEndTag; 
            
            var user = _userManager.FindByNameAsync(Member.Name).Result;
            if((user != null && _userManager.IsInRoleAsync(user,"Administrator").Result) || Member.Level == 3)
            {
                output.Attributes.Add("style", $"color:{Ranking[0].Image} ;");
                for (int i = 0; i < Ranking[0].ImgRepeat; i++)
                {
                    output.Content.AppendHtml($@"<i class=""fa fa-star fs-5""></i>");
                }
            }
            else if((user != null && _userManager.IsInRoleAsync(user,"Moderator").Result) || Member.Level == 2)
            {
                output.Attributes.Add("style", $"color:{Ranking[1].Image} ;");
                for (int i = 0; i < Ranking[1].ImgRepeat; i++)
                {
                    output.Content.AppendHtml($@"<i class=""fa fa-star fs-5""></i>");
                }
            }
            else
            {
                var thisrank = Ranking?.OrderByDescending(i=>i.Value.Posts).FirstOrDefault(r => r.Key > 2 && Member.Posts >= r.Value.Posts);
                if(thisrank.Equals(default(KeyValuePair<int,MemberRanking>)))
                {
                    return;
                }
                try
                {
                    output.Attributes.Add("style", $"color:{thisrank.Value.Value.Image} ;");
                    for (int i = 0; i < thisrank.Value.Value.ImgRepeat; i++)
                    {
                        output.Content.AppendHtml($@"<i class=""fa fa-star fs-5""></i>");
                    }
                }
                catch (Exception)
                {
                    return;
                }
            }
        }
    }
}
