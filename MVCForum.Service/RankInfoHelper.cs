using Newtonsoft.Json.Linq;
using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Text;

namespace SnitzCore.Service
{
    public class RankInfoHelper
    {
        private readonly Dictionary<int, MemberRanking>? _ranking;
        private int _level;
        private int _repeat;
        public string? Title { get; set; }
        private readonly int? _posts;
        private readonly bool _isAdmin;
        private readonly bool _isModerator;
        public string Stars
        {
            get { return GetStars(); }
        }

        public RankInfoHelper(Member? user, ref string? title, int? posts, Dictionary<int, MemberRanking>? rankings)
        {
            _ranking = rankings;
            _posts = posts;
            if (user == null)
            {
                return;
            }

            _isAdmin = user.Level == 3 ;
            _isModerator = user.Level == 2;            
            SetLevel();
            if (string.IsNullOrWhiteSpace(title))
            {
                title = Title;
            }
            else
            {
                Title = title.Trim();
            }

        }

        private string GetStars()
        {
            
            StringBuilder imageString = new("");
            int imageRepeat = _repeat;// _ranking[_Level + 1].Repeat;

            if (_ranking != null)
            {
                string rankImage = _ranking[_level + 1].Image;
                if (_isAdmin)
                {
                    //imageRepeat = _ranking[0].Repeat;
                    rankImage = _ranking[0].Image; //Admin;
                }
                else if (_isModerator)
                {
                    //imageRepeat = _ranking[1].Repeat;
                    rankImage = _ranking[1].Image;
                }
                if (_level == 0) return "";

                if (rankImage != "")
                {
                    for (int ii = 1; ii <= imageRepeat; ii++)
                    {
                        string clientpath =  "/images/rankimages/";
                        if (rankImage.Contains("."))
                        {
                            imageString.AppendFormat("<img src='{0}{1}' alt='ranking image'/>", clientpath, rankImage);
                        }
                        else
                        {
                            if (rankImage.Contains("#"))
                            {
                                imageString.AppendFormat($"<i class='fa fa-star fs-5 ' alt='ranking star' style='color:{rankImage};' ></i>");
                            }
                            else
                            {
                                imageString.AppendFormat($"<i class='fa fa-star fs-5 {rankImage}' alt='ranking star' ></i>");
                            }
                        }
                    }
                }
            }

            return imageString.ToString();
        }
        //private string GetStarsNew()
        //{
            
        //    StringBuilder imageString = new("");
        //    int imageRepeat = _repeat;// _ranking[_Level + 1].Repeat;

        //    if (_ranking != null)
        //    {
        //        string rankImage = _ranking[_level + 1].Image;
        //        if (_isAdmin)
        //        {
        //            //imageRepeat = _ranking[0].Repeat;
        //            rankImage = _ranking[0].Image; //Admin;
        //        }
        //        else if (_isModerator)
        //        {
        //            //imageRepeat = _ranking[1].Repeat;
        //            rankImage = _ranking[1].Image;
        //        }
        //        if (_level == 0) return "";

        //        if (rankImage != "")
        //        {
        //            for (int ii = 1; ii <= imageRepeat; ii++)
        //            {
        //                string clientpath =  "/images/rankimages/";
        //                if (rankImage.Contains("."))
        //                {
        //                    imageString.AppendFormat("<img src='{0}{1}' alt='star'/>", clientpath, rankImage);
        //                }
        //                else
        //                {
        //                    imageString.AppendFormat($"<i class='fa fa-star' alt='star' style='color:{rankImage}'></i>");
        //                }
        //            }
        //        }
        //    }

        //    return imageString.ToString();
        //}
        private void SetLevel()
        {
            _repeat = -1;
            string rankTitle = "";
            _level = 0;

            if (_ranking != null)
                foreach (KeyValuePair<int, MemberRanking> ranking in _ranking)
                {
                    if (ranking.Key < 2)
                        continue;
                    if (_posts >= ranking.Value.Posts)
                    {
                        rankTitle = ranking.Value.Title;
                        _level++;
                        _repeat++;
                    }

                    if (_posts < ranking.Value.Posts)
                        break;
                }

            if (_isAdmin)
            {
                rankTitle = "Forum Administrator";
                _level = -1;
            }
            if (_isModerator)
            {
                rankTitle = "Forum Moderator";
                _level = -1;
            }
            Title = rankTitle;
        }

    }


}
