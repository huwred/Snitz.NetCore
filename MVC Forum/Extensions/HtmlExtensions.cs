using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using MVCForum.Models.User;
using SnitzCore.Data.Models;
using SnitzCore.Service;
using System.Collections.Generic;

namespace MVCForum.Extensions
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
        public static  HtmlString MemberRankTitle(this IHtmlHelper htmlhelper, Member author,
            Dictionary<int, MemberRanking> ranking)
        {

            string mTitle = author.Title;
            if (author.Status == 0 || author.Name == "n/a")
            {
                mTitle =  "Member Locked"; //ResourceManager.GetLocalisedString("tipMemberLocked", "Tooltip");// "Member Locked";
            }
            if (author.Name == "zapped")
            {
                mTitle = "Zapped Member"; //ResourceManager.GetLocalisedString("tipZapped", "Tooltip");// "Zapped Member";
            }
            RankInfoHelper rank = new RankInfoHelper(author, ref mTitle, author.Posts, ranking);
            TagBuilder title = new TagBuilder("span");
            title.AddCssClass("rank-label");
            title.InnerHtml.AppendHtml(mTitle);

            return new HtmlString(title.GetString());
        }
        public static  HtmlString MemberRankStars(this IHtmlHelper htmlhelper, Member author,
            Dictionary<int, MemberRanking> ranking)
        {

            string mTitle = author.Title;
            if (author.Status == 0 || author.Name == "n/a")
            {
                mTitle =  "Member Locked"; //ResourceManager.GetLocalisedString("tipMemberLocked", "Tooltip");// "Member Locked";
            }
            if (author.Name == "zapped")
            {
                mTitle = "Zapped Member"; //ResourceManager.GetLocalisedString("tipZapped", "Tooltip");// "Zapped Member";
            }
            RankInfoHelper rank = new RankInfoHelper(author, ref mTitle, author.Posts, ranking);
            TagBuilder stars = new TagBuilder("span");
            stars.AddCssClass("rank-label");
            stars.InnerHtml.AppendHtml(rank.Stars);

            return new HtmlString(stars.GetString());
        }

        public static object MemberRankStars(this IHtmlHelper htmlhelper, MemberListingModel author, Dictionary<int, MemberRanking> ranking)
        {
            string mTitle = author.Title;
            if (author.Member.Status == 0 || author.Member.Name == "n/a")
            {
                mTitle =  "Member Locked"; //ResourceManager.GetLocalisedString("tipMemberLocked", "Tooltip");// "Member Locked";
            }
            if (author.Member.Name == "zapped")
            {
                mTitle = "Zapped Member"; //ResourceManager.GetLocalisedString("tipZapped", "Tooltip");// "Zapped Member";
            }
            RankInfoHelper rank = new RankInfoHelper(author.Member, ref mTitle, author.Member.Posts, ranking);
            TagBuilder stars = new TagBuilder("span");
            stars.AddCssClass("rank-label");
            stars.InnerHtml.AppendHtml(rank.Stars);

            return new HtmlString(stars.GetString());
        }

        public static string GetString(this IHtmlContent content)
        {
            using var writer = new System.IO.StringWriter();
            content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);
            return writer.ToString();
        }


    }

}
