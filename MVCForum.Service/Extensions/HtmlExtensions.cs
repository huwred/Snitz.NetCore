using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using SnitzCore.Data.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using static Azure.Core.HttpHeader;

namespace SnitzCore.Service.Extensions
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// Renders the Members Rank titles and stars for display in the forum
        /// </summary>
        /// <param name="htmlhelper"></param>
        /// <param name="author"></param>
        /// <param name="ranking"></param>
        /// <returns></returns>
        public static HtmlString MemberRankTitle(this IHtmlHelper htmlhelper, Member author,
            Dictionary<int, MemberRanking>? ranking, IViewLocalizer language)
        {

            string? mTitle = author.Title;
            if (author.Status == 0 || author.Name == "n/a")
            {
                mTitle = language.GetString("tipMemberLocked");
            }
            if (author.Name == "zapped")
            {
                mTitle = language.GetString("tipZapped");
            }
            RankInfoHelper rank = new RankInfoHelper(author, ref mTitle, author.Posts, ranking);
            TagBuilder title = new TagBuilder("span");
            title.AddCssClass("rank-label");
            if (mTitle != null) title.InnerHtml.AppendHtml(mTitle);

            return new HtmlString(title.GetString());
        }
        public static HtmlString MemberRankStars(this IHtmlHelper htmlhelper, Member author,
            Dictionary<int, MemberRanking>? ranking, IViewLocalizer language)
        {

            string? mTitle = author.Title;
            if (author.Status == 0 || author.Name == "n/a")
            {
                mTitle = language.GetString("tipMemberLocked");
            }
            if (author.Name == "zapped")
            {
                mTitle = language.GetString("tipZapped");
            }
            RankInfoHelper rank = new RankInfoHelper(author, ref mTitle, author.Posts, ranking);
            TagBuilder stars = new TagBuilder("span");
            stars.AddCssClass("rank-label");
            stars.InnerHtml.AppendHtml(rank.Stars);

            return new HtmlString(stars.GetString());
        }

        //public static HtmlString MemberRankStars(this IHtmlHelper htmlhelper, MemberListingModel author, Dictionary<int, MemberRanking>? ranking, IViewLocalizer language)
        //{
        //    string? mTitle = author.Title;
        //    if (author.Member.Status == 0 || author.Member.Name == "n/a")
        //    {
        //        mTitle = language.GetString("tipMemberLocked");
        //    }
        //    if (author.Member.Name == "zapped")
        //    {
        //        mTitle = language.GetString("tipZapped");
        //    }
        //    RankInfoHelper rank = new RankInfoHelper(author.Member, ref mTitle, author.Member.Posts, ranking);
        //    TagBuilder stars = new TagBuilder("span");
        //    stars.AddCssClass("rank-label");
        //    stars.InnerHtml.AppendHtml(rank.Stars);

        //    return new HtmlString(stars.GetString());
        //}

        public static string GetString(this IHtmlContent content)
        {
            using var writer = new System.IO.StringWriter();
            content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
            return writer.ToString();
        }
        public static HtmlString ShowHideForums(this IHtmlHelper helper, IPrincipal user, Forum model, IEnumerable<string> roles, IViewLocalizer language, string wrapper = "{0}")
        {
            if (wrapper == "li")
            {
                wrapper = "<li class=\"list-unstyled\">{0}</li>";
            }
            var baseUrl = helper.ViewContext.HttpContext.Request.PathBase;
            string forumlink = $"<a data-title='{language.GetString("tipForumViewPosts", "Tooltip")}' data-toggle='tooltip' class='forum-link' href='{baseUrl}/Forum/{model.Id}?pagenum=1' >{model.Title}</a>";
            string archivelink = "&nbsp;<a data-title='" + language.GetString("tipForumViewArchived", "Tooltip") + "' data-toggle='tooltip' href='" + baseUrl + "/Forum/" + model.Id + "?pagenum=1&archived=1' ><i class='fa fa-file-text'></i></a>";
            if (model.ArchivedCount > 0 && user.Identity.IsAuthenticated)
            {
                forumlink += archivelink;
            }

            if (user.IsAdministrator())
            { return new HtmlString(String.Format(wrapper, forumlink)); }
            if (user.CanViewForum(model, roles))
            {
                return new HtmlString(String.Format(wrapper, forumlink));
            }

            return new HtmlString(String.Empty);
        }

        public static bool CanViewCategory(this IPrincipal user, Category cat, IEnumerable<string> roles)
        {
            var key = user.Identity?.Name + "_C" + cat.Id;
            var result = CacheProvider.GetOrCreate(key, () => CanViewCategoryCache(user, cat, roles), TimeSpan.FromMinutes(10));

            return result;
        }
        private static bool CanViewCategoryCache(this IPrincipal user, Category cat, IEnumerable<string> roles)
        {
            //var key = user.Identity.Name + "_" + cat.Id;

            bool canview = user.IsInRole("Administrator") || cat.Forums.All(f => f.Privateforums == ForumAuthType.All);

            var publicview = new[] { ForumAuthType.All, ForumAuthType.PasswordProtected };
            var hidden = new[] { ForumAuthType.MembersHidden, ForumAuthType.AllowedMembersHidden };
            var members = new[] { ForumAuthType.Members, ForumAuthType.MembersPassword, ForumAuthType.AllowedMembers, ForumAuthType.AllowedMemberPassword };

            foreach (var forum in cat.Forums.Where(f=>f.Privateforums != ForumAuthType.All))
            {
                if (forum.Privateforums.In(publicview))
                {
                    canview = true;
                }
                else if (forum.Privateforums.In(members))
                {
                    canview = true;
                }
                if (user.Identity.IsAuthenticated)
                {
                    if (roles != null)
                    {
                        foreach (string role in roles)
                        {
                            if (user.IsInRole(role))
                            {
                                canview = true;
                            }
                        }
                    }
                    else
                    {
                        if (forum.Privateforums.In(hidden))
                        {
                            if (user.CanViewForum(forum, null))
                                canview = true; ;
                            //return false;
                        }
                    }
                }

            }

            return canview;
        }
        public static bool CanViewForum(this IPrincipal user, Forum forum, IEnumerable<string> roles)
        {
            var key = user.Identity?.Name + "_F" + forum.Id;
            var result = CacheProvider.GetOrCreate(key, () => CanViewForumCache(user, forum, roles), TimeSpan.FromMinutes(10));

            return result;
        }
        private static bool CanViewForumCache(this IPrincipal user, Forum forum, IEnumerable<string> roles)
        {

            bool canview = false;

            switch (forum.Privateforums)
            {
                case ForumAuthType.All:
                    canview = true;
                    break;
                case ForumAuthType.AllowedMembers:
                case ForumAuthType.AllowedMemberPassword:
                    if (user.IsInRole("Forum_" + forum.Id) || user.IsInRole("FORUM_" + forum.Id))
                    {
                        canview = true;
                    }
                    break;
                case ForumAuthType.MembersHidden:
                case ForumAuthType.AllowedMembersHidden:
                        canview = false;
                    break;
                case ForumAuthType.Members:
                case ForumAuthType.MembersPassword:
                case ForumAuthType.PasswordProtected:
                    canview = user.Identity.IsAuthenticated;
                    break;
            }

            if (user.IsAdministrator())
            {
                canview = true;
            }

            return canview;
        }
        public static bool In<T>(this T val, params T[] values) where T : struct
        {
            return values.ToList().Contains(val);
        }
    }

}
