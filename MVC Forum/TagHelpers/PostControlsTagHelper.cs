using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace MVCForum.TagHelpers
{
    [HtmlTargetElement("post-controls", TagStructure = TagStructure.NormalOrSelfClosing)]
    public class PostControlsTagHelper : TagHelper
    {
        [HtmlAttributeName("logged-in")]
        public bool LoggedIn { get; set; }
        [HtmlAttributeName("post-author")]
        public string Author { get; set; }
        [HtmlAttributeName("curr-user")]
        public ClaimsPrincipal User { get; set; }
        [HtmlAttributeName("post-id")]
        public int PostId { get; set; }
        [HtmlAttributeName("topic-id")]
        public int? TopicId { get; set; }
        [HtmlAttributeName("post-type")]
        public PostType Posttype { get; set; }
        [HtmlAttributeName("post-status")]
        public bool IsLocked { get; set; }
        private readonly UserManager<ForumUser> _userManager;

        public PostControlsTagHelper(UserManager<ForumUser> userManager)
        {
            _userManager = userManager;
        }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            base.Process(context, output);
            string username = "";
            IList<string>? userroles = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var curruser = _userManager.GetUserAsync(User).Result;
                userroles = _userManager.GetRolesAsync(curruser).Result;
                username = User.Identity.Name;
            }
            output.TagName = "div";
            output.AddClass("post-control-btn",HtmlEncoder.Default);
            if (LoggedIn)
            {
                var isadmin = userroles!.Contains("Admin");
                if(!IsLocked || isadmin)
                    output.Content.AppendHtml($@"<i class=""fa fa-comment-o m-1 post-reply"" title=""Reply to Topic"" data-id=""{TopicId??PostId}""></i>");
                if (username == Author || isadmin)
                {
                    if (Posttype == PostType.Topic)
                    {
                        if (!IsLocked || isadmin)
                        {
                            output.Content.AppendHtml($@"<i class=""fa fa-commenting m-1 post-quote"" title=""Reply with Quote"" data-id=""{PostId}""></i>");
                            output.Content.AppendHtml($@"<i class=""fa fa-pencil m-1 post-edit"" title=""Edit Post"" data-id=""{PostId}""></i>");
                        }
                        if (isadmin)
                        {
                            output.Content.AppendHtml($@"<i class=""fa fa-trash m-1 post-del"" title=""Delete Post"" data-id=""{PostId}""></i>");
                            output.Content.AppendHtml(IsLocked
                                ? $@"<i class=""fa fa-unlock admin m-1 post-lock"" title=""UnLock Post"" data-id=""{PostId}"" data-status=""1""></i>"
                                : $@"<i class=""fa fa-lock admin m-1 post-lock"" title=""Lock Post"" data-id=""{PostId}"" data-status=""0""></i>");
                        }
                    }
                    else
                    {
                        if (!IsLocked || isadmin)
                        {
                            output.Content.AppendHtml($@"<i class=""fa fa-commenting m-1 reply-quote"" title=""Reply with Quote"" data-id=""{PostId}""></i>");
                            output.Content.AppendHtml($@"<i class=""fa fa-pencil m-1 reply-edit"" title=""Edit Post"" data-id=""{PostId}""></i>");
                        }
                        if (isadmin)
                        {
                            output.Content.AppendHtml($@"<i class=""fa fa-trash m-1 reply-del"" title=""Delete Post"" data-id=""{PostId}""></i>");
                            output.Content.AppendHtml(IsLocked
                                ? $@"<i class=""fa fa-unlock admin m-1 post-lock"" title=""UnLock Post"" data-id=""{TopicId}"" data-status=""1""></i>"
                                : $@"<i class=""fa fa-lock admin m-1 post-lock"" title=""Lock Post"" data-id=""{TopicId}"" data-status=""0""></i>");
                        }
                    }

                }
            }

            output.TagMode = TagMode.StartTagAndEndTag;
        }
    }
}
