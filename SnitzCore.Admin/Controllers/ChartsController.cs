using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using SnitzCore.BackOffice.ViewModels;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SnitzCore.BackOffice.Controllers
{
    [Authorize(Roles = "Administrator, Moderator")]
    public class ChartsController : Controller
    {
        private readonly SnitzDbContext _context;
        private readonly IOptions<SnitzForums> _config;
        private bool showVisits = false;

        private string thanksPostSql = "";
        private string thanksReplySql = "";
        private string thanksGivenSql = "";
        public ChartsController(SnitzDbContext context,IOptions<SnitzForums> config,IConfiguration settings)
        {
            _context = context;
            _config = config;

            thanksPostSql = $@"SELECT t.*,m.* FROM {_config.Value.forumTablePrefix}THANKS ft
                JOIN {_config.Value.forumTablePrefix}TOPICS t on ft.TOPIC_ID = t.TOPIC_ID
                LEFT JOIN {_config.Value.forumTablePrefix}REPLY r on ft.REPLY_ID = r.REPLY_ID
				JOIN {_config.Value.memberTablePrefix}MEMBERS m on m.MEMBER_ID = t.T_AUTHOR";
            thanksReplySql = $@"
                SELECT r.*,m.* FROM {_config.Value.forumTablePrefix}THANKS ft
                JOIN {_config.Value.forumTablePrefix}TOPICS t on ft.TOPIC_ID = t.TOPIC_ID
                LEFT JOIN {_config.Value.forumTablePrefix}REPLY r on ft.REPLY_ID = r.REPLY_ID
				JOIN {_config.Value.memberTablePrefix}MEMBERS m on m.MEMBER_ID = r.R_AUTHOR";     
            thanksGivenSql = $@"
                SELECT m.* FROM {_config.Value.forumTablePrefix}THANKS ft
				JOIN {_config.Value.memberTablePrefix}MEMBERS m on m.MEMBER_ID = ft.MEMBER_ID";  
            
            showVisits = settings.GetValue<string>("SnitzForums:VisitorTracking","") != "";
        }
        public IActionResult Index()
        {
            var yearQuery = @"select min(SUBSTRING(T_DATE, 1, 4)) as MinYear, max(SUBSTRING(T_DATE, 1, 4)) as MaxYear FROM " + _config.Value.forumTablePrefix + "TOPICS";
            FormattableString formattableQuery = FormattableStringFactory.Create(yearQuery);
            ChartModel? Years = _context.Database.SqlQuery<ChartModel>(formattableQuery).FirstOrDefault();
            return View(Years);

        }
        public IActionResult LeaderBoard()
        {
            return View();
        }
        [HttpGet]
        [ResponseCache(Duration = 200, Location = ResponseCacheLocation.Any,VaryByQueryKeys = new[] { "today" } )]
        public IActionResult UserLeaderboard(bool today = false)
        {
            var todayStr = DateTime.UtcNow.ToString("yyyyMMdd");
            var topics = _context.Posts.AsNoTracking().Include(p=>p.Member).AsEnumerable() ?? Enumerable.Empty<Post>();
            var replies = _context.Replies.AsNoTracking().Include(p=>p.Member).AsEnumerable() ?? Enumerable.Empty<PostReply>();
            var visitors = _context.VisitorLog.AsNoTracking().AsEnumerable() ?? Enumerable.Empty<VisitorLog>();

            var thanksPostSql = this.thanksPostSql + $@" WHERE r.R_AUTHOR IS NULL ";
            var thanksReplySql = this.thanksReplySql + $@" WHERE r.R_AUTHOR IS NOT NULL";
            string thanksGivenSql = this.thanksGivenSql;

            if (today)
            {
                replies = replies.Where(r => r.Created.Substring(0,8) == todayStr);
                topics = topics.Where(r => r.Created.Substring(0,8) == todayStr);
                visitors = visitors.Where(r => r.VisitTime.Date == DateTime.UtcNow.Date);
                thanksPostSql += $@" AND ft.THANKS_DATE IS NOT NULL AND SUBSTRING(ft.THANKS_DATE, 1, 8) = '{todayStr}'";
                thanksReplySql += $@" AND ft.THANKS_DATE IS NOT NULL AND SUBSTRING(ft.THANKS_DATE, 1, 8) = '{todayStr}'";
                thanksGivenSql += $@" WHERE ft.THANKS_DATE IS NOT NULL AND SUBSTRING(ft.THANKS_DATE, 1, 8) = '{todayStr}'";
            }
            var thanksPost = _context.Posts.FromSqlInterpolated(FormattableStringFactory.Create(thanksPostSql))
                .Select(p => new Post
                {
                    // Map Post properties
                    Id = p.Id,
                    Member = new Member
                    {
                        // Map Member properties
                        Id = p.Member.Id, 
                        Name = p.Member.Name
                    }
                })
                .AsEnumerable() ?? Enumerable.Empty<Post>();
            var thanksReply = _context.Replies.FromSqlInterpolated(FormattableStringFactory.Create(thanksReplySql))
                .Select(p => new PostReply
                {
                    // Map Post properties
                    Id = p.Id,
                    Member = new Member
                    {
                        // Map Member properties
                        Id = p.Member.Id, 
                        Name = p.Member.Name
                    }
                })                
                .AsEnumerable() ?? Enumerable.Empty<PostReply>();
            var thanksGiven = _context.Members.FromSqlInterpolated(FormattableStringFactory.Create(thanksGivenSql))
                .AsEnumerable() ?? Enumerable.Empty<Member>();
            IEnumerable<dynamic> leaderboard = null;
            try
            {
                leaderboard = 
                    //replies
                    replies.Where(r => r.Answer == true)
                    .GroupBy(t => t.Member.Name)
                    .Select(g => new
                    {
                        UserName = g.Key,
                        Answers = g.Count(),
                        Replies = 0,
                        Thanks = 0,
                        Posts = 0
                    })
                    .Concat(
                        replies.Where(r => r.Answer != true)
                        .GroupBy(r => r.Member!.Name)
                        .Select(g => new
                        {
                            UserName = g.Key,
                            Answers = 0,
                            Replies = g.Count(),
                            Thanks = 0,
                            Posts = 0
                        })
                    )
                    //topics
                    .Concat(
                        topics
                        .Where(r => r.Answered == true)
                        .GroupBy(r => r.Member.Name)
                        .Select(g => new
                        {
                            UserName = g.Key,
                            Answers = g.Count(),
                            Replies = 0,
                            Thanks = 0,
                            Posts = 0
                        })
                    )
                    .Concat(
                        topics
                        .Where(r => r.Answered != true && (r.ReplyCount > 10 || r.ViewCount > 100 || r.RatingTotalCount > 0))
                        .GroupBy(r => r.Member.Name)
                        .Select(g => new
                        {
                            UserName = g.Key,
                            Answers = 0,
                            Replies = 0,
                            Thanks = 0,
                            Posts = g.Count()
                        })
                    )
                    //visits
                    .Concat(
                        visitors.Where(v => !string.IsNullOrWhiteSpace(v.UserName) && !v.UserName.StartsWith("Anonymous"))
                        .GroupBy(v => v.UserName, StringComparer.OrdinalIgnoreCase)
                        .Select(g => new
                        {
                            UserName = g.Key!,
                            Answers = 0,
                            Replies = 0,
                            Thanks = 0,
                            Posts = g.Count() / 10 // 1 point for every 10 visits
                        })
                    )
                    //thanks
                    .Concat(
                        thanksPost
                        .GroupBy(t => t.Member!.Name)
                        .Select(g => new
                        {
                            UserName = g.Key,
                            Answers = 0,
                            Replies = 0,
                            Thanks = g.Count(),
                            Posts = 0
                        })
                        .Concat(
                            thanksReply
                            .GroupBy(t => t.Member!.Name)
                            .Select(g => new
                            {
                                UserName = g.Key,
                                Answers = 0,
                                Replies = 0,
                                Thanks = g.Count(),
                                Posts = 0
                            })
                        )
                        .Concat(
                            thanksGiven
                            .GroupBy(m => m.Name)
                            .Select(g => new
                            {
                                UserName = g.Key,
                                Answers = 0,
                                Replies = 0,
                                Thanks = g.Count(),
                                Posts = 0
                            })
                        )
                    )
                    .GroupBy(x => x.UserName, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new
                    {
                        UserName = g.Key,
                        Answers = g.Sum(x => x.Answers),
                        Replies = g.Sum(x => x.Replies),
                        Thanks = g.Sum(x => x.Thanks),
                        Posts = g.Sum(x => x.Posts),
                        Total = g.Sum(x => x.Answers) * 3 + g.Sum(x => x.Replies) + g.Sum(x => x.Thanks) * 2 + g.Sum(x => x.Posts)
                    })
                    .OrderByDescending(x => x.Total)
                    .Take(50)
                    .ToList();
            }
            catch (Exception e)
            {

                throw;
            }


            if (!today)
            {
                try
                {
                    leaderboard = leaderboard
                    .Concat(
                        _context.ArchivedTopics.AsNoTracking().Include(p=>p.Member).AsEnumerable()
                        .Where(r => (r.ReplyCount > 10 || r.ViewCount > 100 || r.RatingTotalCount > 0))
                        .GroupBy(r => r.Member!.Name, StringComparer.OrdinalIgnoreCase)
                        .Select(g => new
                        {
                            UserName = g.Key,
                            Answers = 0,
                            Replies = 0,
                            Thanks = 0,
                            Posts = g.Count(),
                            Total = 0
                        })
                    )
                    .GroupBy(x => x.UserName)
                    .Select(g => new
                    {
                        UserName = g.Key,
                        Answers = g.Sum(x => x.Answers),
                        Replies = g.Sum(x => x.Replies),
                        Thanks = g.Sum(x => x.Thanks),
                        Posts = g.Sum(x => x.Posts),
                        Total = g.Sum(x => x.Answers) * 3 + g.Sum(x => x.Replies) + g.Sum(x => x.Thanks) * 2 + g.Sum(x => x.Posts)
                    })
                    .OrderByDescending(x => x.Total)
                    .Take(50)
                    .ToList();
                }
                catch (Exception e)
                {

                    throw;
                }

            }


            // Assign ranks: same rank for same total, next rank skips accordingly
            int currentRank = 1;
            int previousTotal = -1;

            var rankedLeaderboard = leaderboard
                .Select((item, index) =>
                {
                    if (item.Total != previousTotal)
                    {
                        currentRank ++;

                    }

                    previousTotal = item.Total;
                    return new
                    {
                        Rank = currentRank-1,
                        item.UserName,
                        item.Answers,
                        item.Replies,
                        item.Thanks,
                        item.Total
                    };
                })
                .ToList();
            return Json(rankedLeaderboard);
        }

        [HttpGet]
        [ResponseCache(Duration = 200, Location = ResponseCacheLocation.Any,VaryByQueryKeys = new[] { "month" } )]
        public IActionResult UserLeaderboardByMonth(int year, int month)
        {
            string thanksPostSql = this.thanksPostSql + $@" WHERE r.R_AUTHOR IS NULL AND ft.THANKS_DATE IS NOT NULL AND SUBSTRING(ft.THANKS_DATE, 1, 4) = '{year}' ";
            string thanksReplySql = this.thanksReplySql + $@" WHERE r.R_AUTHOR IS NOT NULL AND ft.THANKS_DATE IS NOT NULL AND SUBSTRING(ft.THANKS_DATE, 1, 4) = '{year}' ";
            string thanksGivenSql = this.thanksGivenSql + $@" WHERE ft.THANKS_DATE IS NOT NULL AND SUBSTRING(ft.THANKS_DATE, 1, 4) = '{year}' ";

            var replies = _context.Replies.AsNoTracking().Include(p=>p.Member).Where(r => Convert.ToInt32(r.Created.Substring(0,4)) == year).AsEnumerable() ?? Enumerable.Empty<PostReply>();
            var topics = _context.Posts.AsNoTracking().Include(p=>p.Member).Where(t => Convert.ToInt32(t.Created.Substring(0,4)) == year).AsEnumerable() ?? Enumerable.Empty<Post>();
            var visitors = _context.VisitorLog.AsNoTracking().Where(l=>l.VisitTime.Year == year).AsEnumerable() ?? Enumerable.Empty<VisitorLog>();

            if (month > 0)
            {
                replies = replies.Where(r => Convert.ToInt32(r.Created.Substring(4,2)) == month);
                topics = topics.Where(t => Convert.ToInt32(t.Created.Substring(4,2)) == month);
                visitors = visitors.Where(l => l.VisitTime.Month == month);
                thanksPostSql += $@" AND SUBSTRING(ft.THANKS_DATE, 5, 2) = '{month}'";
                thanksReplySql += $@" AND SUBSTRING(ft.THANKS_DATE, 5, 2) = '{month}'";
                thanksGivenSql += $@" AND SUBSTRING(ft.THANKS_DATE, 5, 2) = '{month}'";
            }
            var thanksPost = _context.Posts.FromSqlInterpolated(FormattableStringFactory.Create(thanksPostSql))
                .Select(p => new Post
                {
                    // Map Post properties
                    Id = p.Id,
                    Member = new Member
                    {
                        // Map Member properties
                        Id = p.Member.Id, 
                        Name = p.Member.Name
                    }
                })
                .AsEnumerable() ?? Enumerable.Empty<Post>();
            var thanksReply = _context.Replies.FromSqlInterpolated(FormattableStringFactory.Create(thanksReplySql))
                .Select(p => new PostReply
                {
                    // Map Post properties
                    Id = p.Id,
                    Member = new Member
                    {
                        // Map Member properties
                        Id = p.Member.Id, 
                        Name = p.Member.Name
                    }
                })                
                .AsEnumerable() ?? Enumerable.Empty<PostReply>();
            var thanksGiven = _context.Members.FromSqlInterpolated(FormattableStringFactory.Create(thanksGivenSql))
                .AsEnumerable() ?? Enumerable.Empty<Member>();
            IEnumerable<dynamic> leaderboard = null;
            try
            {
                leaderboard = 
                //replies
                replies.Where(r => r.Answer == true)
                .GroupBy(t => t.Member.Name)
                .Select(g => new
                {
                    UserName = g.Key,
                    Answers = g.Count(),
                    Replies = 0,
                    Thanks = 0,
                    Posts = 0
                })
                .Concat(
                    replies.Where(r => r.Answer != true)
                    .GroupBy(r => r.Member!.Name)
                    .Select(g => new
                    {
                        UserName = g.Key,
                        Answers = 0,
                        Replies = g.Count(),
                        Thanks = 0,
                        Posts = 0
                    })
                )
                //topics
                .Concat(
                    topics
                    .Where(r => r.Answered == true)
                    .GroupBy(r => r.Member.Name)
                    .Select(g => new
                    {
                        UserName = g.Key,
                        Answers = g.Count(),
                        Replies = 0,
                        Thanks = 0,
                        Posts = 0
                    })
                )
                .Concat(
                    topics
                    .Where(r => r.Answered != true && (r.ReplyCount > 10 || r.ViewCount > 100 || r.RatingTotalCount > 0))
                    .GroupBy(r => r.Member.Name)
                    .Select(g => new
                    {
                        UserName = g.Key,
                        Answers = 0,
                        Replies = 0,
                        Thanks = 0,
                        Posts = g.Count()
                    })
                )
                //visits
                .Concat(
                    visitors.Where(v => !string.IsNullOrWhiteSpace(v.UserName) && !v.UserName.StartsWith("Anonymous"))
                    .GroupBy(v => v.UserName, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new
                    {
                        UserName = g.Key!,
                        Answers = 0,
                        Replies = 0,
                        Thanks = 0,
                        Posts = g.Count() / 10 // 1 point for every 10 visits
                    })
                )
                //thanks
                .Concat(
                    thanksPost
                    .GroupBy(t => t.Member.Name)
                    .Select(g => new
                    {
                        UserName = g.Key,
                        Answers = 0,
                        Replies = 0,
                        Thanks = g.Count(),
                        Posts = 0
                    })
                    .Concat(
                        thanksReply
                        .GroupBy(t => t.Member!.Name)
                        .Select(g => new
                        {
                            UserName = g.Key,
                            Answers = 0,
                            Replies = 0,
                            Thanks = g.Count(),
                            Posts = 0
                        })
                    )
                    .Concat(
                        thanksGiven
                        .GroupBy(m => m.Name)
                        .Select(g => new
                        {
                            UserName = g.Key,
                            Answers = 0,
                            Replies = 0,
                            Thanks = g.Count(),
                            Posts = 0
                        })
                    )
                )
                .GroupBy(x => x.UserName, StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    UserName = g.Key,
                    Answers = g.Sum(x => x.Answers),
                    Replies = g.Sum(x => x.Replies),
                    Thanks = g.Sum(x => x.Thanks),
                    Posts = g.Sum(x => x.Posts),
                    Total = g.Sum(x => x.Answers)*3 + g.Sum(x => x.Replies) + g.Sum(x => x.Thanks)*2 + g.Sum(x => x.Posts)
                })
                .OrderByDescending(x => x.Total)
                .Take(50)
                .ToList();
            }
            catch (Exception e)
            {

                throw;
            }


            // Assign ranks: same rank for same total, next rank skips accordingly
            int currentRank = 1;
            int previousTotal = -1;

            var rankedLeaderboard = leaderboard
                .Select((item, index) =>
                {
                    if (item.Total != previousTotal)
                    {
                        currentRank ++;

                    }

                    previousTotal = item.Total;
                    return new
                    {
                        Rank = currentRank-1,
                        item.UserName,
                        item.Answers,
                        item.Replies,
                        item.Thanks,
                        item.Total
                    };
                })
                .ToList();
            return Json(rankedLeaderboard);
        }

        [HttpGet]
        [ResponseCache(Duration = 200, Location = ResponseCacheLocation.Any,VaryByQueryKeys = new[] { "week" } )]
        public IActionResult UserLeaderboardByWeek(int year, int week)
        {

            var replies = _context.Replies.AsNoTracking().Include(p=>p.Member).AsEnumerable().Where(r => Convert.ToInt32(r.Created.Substring(0,4)) == year && r.Created.GetIso8601WeekOfYear() == week) ?? Enumerable.Empty<PostReply>();
            var topics = _context.Posts.AsNoTracking().Include(p=>p.Member).AsEnumerable().Where(t => Convert.ToInt32(t.Created.Substring(0,4)) == year && t.Created.GetIso8601WeekOfYear() == week) ?? Enumerable.Empty<Post>();
            var visitors = _context.VisitorLog.AsNoTracking().AsEnumerable().Where(l=>l.VisitTime.Year == year && l.VisitTime.GetIso8601WeekOfYear() == week) ?? Enumerable.Empty<VisitorLog>();

            string thanksPostSql = this.thanksPostSql + $@" WHERE ft.THANKS_DATE IS NOT NULL AND r.R_AUTHOR IS NULL AND SUBSTRING(ft.THANKS_DATE, 1, 4) = '{year}' AND DATEPART(ISO_WEEK, CONCAT(SUBSTRING(ft.THANKS_DATE, 1, 4) , '-', SUBSTRING(ft.THANKS_DATE, 5, 2) , '-' , SUBSTRING(ft.THANKS_DATE, 7, 2))) = {week}";

            string thanksReplySql = this.thanksReplySql + $@" WHERE ft.THANKS_DATE IS NOT NULL AND r.R_AUTHOR IS NOT NULL AND SUBSTRING(ft.THANKS_DATE, 1, 4) = '{year}' AND DATEPART(ISO_WEEK, CONCAT(SUBSTRING(ft.THANKS_DATE, 1, 4) , '-', SUBSTRING(ft.THANKS_DATE, 5, 2) , '-' , SUBSTRING(ft.THANKS_DATE, 7, 2))) = {week}";

            string thanksGivenSql = this.thanksGivenSql + $@" WHERE ft.THANKS_DATE IS NOT NULL AND SUBSTRING(ft.THANKS_DATE, 1, 4) = '{year}' AND DATEPART(ISO_WEEK, CONCAT(SUBSTRING(ft.THANKS_DATE, 1, 4) , '-', SUBSTRING(ft.THANKS_DATE, 5, 2) , '-' , SUBSTRING(ft.THANKS_DATE, 7, 2))) = {week}";

            var thanksPost = _context.Posts.FromSqlInterpolated(FormattableStringFactory.Create(thanksPostSql))
                .Select(p => new Post
                {
                    // Map Post properties
                    Id = p.Id,
                    Member = new Member
                    {
                        // Map Member properties
                        Id = p.Member.Id, 
                        Name = p.Member.Name
                    }
                })
                .AsEnumerable() ?? Enumerable.Empty<Post>();
            var thanksReply = _context.Replies.FromSqlInterpolated(FormattableStringFactory.Create(thanksReplySql))
                .Select(p => new PostReply
                {
                    // Map Post properties
                    Id = p.Id,
                    Member = new Member
                    {
                        // Map Member properties
                        Id = p.Member.Id, 
                        Name = p.Member.Name
                    }
                })                
                .AsEnumerable() ?? Enumerable.Empty<PostReply>();
            var thanksGiven = _context.Members.FromSqlInterpolated(FormattableStringFactory.Create(thanksGivenSql))
                .AsEnumerable() ?? Enumerable.Empty<Member>();
            IEnumerable<dynamic> leaderboard = null;

            try
            {
                leaderboard = 
                    //replies
                    replies.Where(r => r.Answer == true)
                    .GroupBy(t => t.Member.Name)
                    .Select(g => new
                    {
                        UserName = g.Key,
                        Answers = g.Count(),
                        Replies = 0,
                        Thanks = 0,
                        Posts = 0
                    })
                    .Concat(
                        replies.Where(r => r.Answer != true)
                        .GroupBy(r => r.Member!.Name)
                        .Select(g => new
                        {
                            UserName = g.Key,
                            Answers = 0,
                            Replies = g.Count(),
                            Thanks = 0,
                            Posts = 0
                        })
                    )
                    //topics
                    .Concat(
                        topics
                        .Where(r => r.Answered == true)
                        .GroupBy(r => r.Member.Name)
                        .Select(g => new
                        {
                            UserName = g.Key,
                            Answers = g.Count(),
                            Replies = 0,
                            Thanks = 0,
                            Posts = 0
                        })
                    )
                    .Concat(
                        topics
                        .Where(r => r.Answered != true && (r.ReplyCount > 10 || r.ViewCount > 100 || r.RatingTotalCount > 0))
                        .GroupBy(r => r.Member.Name)
                        .Select(g => new
                        {
                            UserName = g.Key,
                            Answers = 0,
                            Replies = 0,
                            Thanks = 0,
                            Posts = g.Count()
                        })
                    )
                    //visits
                    .Concat(
                        visitors.Where(v => !string.IsNullOrWhiteSpace(v.UserName) && !v.UserName.StartsWith("Anonymous"))
                        .GroupBy(v => v.UserName, StringComparer.OrdinalIgnoreCase)
                        .Select(g => new
                        {
                            UserName = g.Key!,
                            Answers = 0,
                            Replies = 0,
                            Thanks = 0,
                            Posts = g.Count() / 10 // 1 point for every 10 visits
                        })
                    )
                    //thanks
                    .Concat(
                        thanksPost
                        .GroupBy(t => t.Member!.Name)
                        .Select(g => new
                        {
                            UserName = g.Key,
                            Answers = 0,
                            Replies = 0,
                            Thanks = g.Count(),
                            Posts = 0
                        })
                        .Concat(
                            thanksReply
                            .GroupBy(t => t.Member!.Name)
                            .Select(g => new
                            {
                                UserName = g.Key,
                                Answers = 0,
                                Replies = 0,
                                Thanks = g.Count(),
                                Posts = 0
                            })
                        )
                        .Concat(
                            thanksGiven
                            .GroupBy(m => m.Name)
                            .Select(g => new
                            {
                                UserName = g.Key,
                                Answers = 0,
                                Replies = 0,
                                Thanks = g.Count(),
                                Posts = 0
                            })
                        )
                    )
                    .GroupBy(x => x.UserName, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new
                    {
                        UserName = g.Key,
                        Answers = g.Sum(x => x.Answers),
                        Replies = g.Sum(x => x.Replies),
                        Thanks = g.Sum(x => x.Thanks),
                        Posts = g.Sum(x => x.Posts),
                        Total = g.Sum(x => x.Answers)*3 + g.Sum(x => x.Replies) + g.Sum(x => x.Thanks)*2 + g.Sum(x => x.Posts)
                    })
                    .OrderByDescending(x => x.Total)
                    .Take(50)
                    .ToList();
            }
            catch (Exception e)
            {

                throw;
            }

                // Assign ranks: same rank for same total, next rank skips accordingly

                int currentRank = 1;
                int previousTotal = -1;

            var rankedLeaderboard = leaderboard
                    .Select((item, index) =>
                    {
                        if (item.Total != previousTotal)
                        {
                            currentRank++;

                        }

                        previousTotal = item.Total;
                        return new
                        {
                            Rank = currentRank - 1,
                            item.UserName,
                            item.Answers,
                            item.Replies,
                            item.Thanks,
                            item.Total
                        };
                    })
                    .ToList();
            return Json(rankedLeaderboard);
        }


        public JsonResult PostsByYear(string page)
        {

            var postsByYear =
                            $@"
            SELECT 
            (SELECT COUNT(*) FROM {_config.Value.forumTablePrefix}REPLY WHERE  SUBSTRING(R_DATE, 1, 4) = SUBSTRING(A.T_DATE, 1, 4)) + 
            (SELECT COUNT(*) FROM {_config.Value.forumTablePrefix}A_REPLY WHERE  SUBSTRING(R_DATE, 1, 4) = SUBSTRING(A.T_DATE, 1, 4)) + 
            (SELECT COUNT(*) FROM {_config.Value.forumTablePrefix}TOPICS WHERE  SUBSTRING(T_DATE, 1, 4) = SUBSTRING(A.T_DATE, 1, 4)) + 
            (SELECT COUNT(*) FROM {_config.Value.forumTablePrefix}A_TOPICS WHERE  SUBSTRING(T_DATE, 1, 4) = SUBSTRING(A.T_DATE, 1, 4)) AS 'Value',
            SUBSTRING(A.T_DATE, 1, 4) AS 'Key'
            FROM {_config.Value.forumTablePrefix}TOPICS A
            GROUP BY SUBSTRING(A.T_DATE, 1, 4)

            ";
            //ORDER BY SUBSTRING(A.T_DATE, 1, 4)

            var data = _context.Database.SqlQueryRaw<Pair<string, Int32>>(postsByYear).OrderBy(m=> m.Key.Substring(0, 4));
            List<object> iData = new List<object>
            {
                data.Select(m => m.Key).ToList(), data.Select(m => m.Value).ToList()
            };

            return Json(iData);
        }
        //[HttpPost]
        public JsonResult PostsByMonth(int id, int? page)
        {
            var postsbyMonth =
                $@"
            SELECT 
            (SELECT COUNT(*) FROM {_config.Value.forumTablePrefix}REPLY WHERE  SUBSTRING(R_DATE, 1, 6) = SUBSTRING(A.T_DATE, 1, 6)) + 
            (SELECT COUNT(*) FROM {_config.Value.forumTablePrefix}A_REPLY WHERE  SUBSTRING(R_DATE, 1, 6) = SUBSTRING(A.T_DATE, 1, 6)) + 
            (SELECT COUNT(*) FROM {_config.Value.forumTablePrefix}TOPICS WHERE  SUBSTRING(T_DATE, 1, 6) = SUBSTRING(A.T_DATE, 1, 6)) + 
            (SELECT COUNT(*) FROM {_config.Value.forumTablePrefix}A_TOPICS WHERE  SUBSTRING(T_DATE, 1, 6) = SUBSTRING(A.T_DATE, 1, 6)) AS 'Value',
            SUBSTRING(A.T_DATE, 5, 2) AS 'Key'
            FROM {_config.Value.forumTablePrefix}TOPICS A
            WHERE  SUBSTRING(A.T_DATE, 1, 4) = {id}
            GROUP BY SUBSTRING(A.T_DATE, 1, 6), SUBSTRING(A.T_DATE, 5, 2)
            ";
            //ORDER BY SUBSTRING(A.T_DATE, 1, 6), SUBSTRING(A.T_DATE, 5, 2)

            try
            {
                var data = _context.Database.SqlQueryRaw<Pair<string, int>>(postsbyMonth).OrderBy(m=> m.Key.Substring(0, 4)).ThenBy(m=> m.Key.Substring(4, 2));
                List<object> iData = new List<object>
                {
                    data.Select(m => m.Key).ToList(), 
                    data.Select(m => m.Value).ToList()
                };
                return Json(iData);
            }
            catch (Exception e)
            {
                ////Console.WriteLine(e);
                throw;
            }
            
        }

        //[HttpPost]
        public JsonResult TopicsByUser()
        {
            var sqlTop = "";
            if (_context.Database.IsSqlServer())
            {
                sqlTop = "TOP 10";
            }

            var postsbyMember = $@"select {sqlTop} poster  as 'Key', CAST(sum(postcount) as INT) as 'Value' 
                  from (
                    select M.M_NAME AS poster, count(*) as postcount
                      from {_config.Value.forumTablePrefix}TOPICS
	                  JOIN {_config.Value.memberTablePrefix}MEMBERS M ON T_AUTHOR = M.MEMBER_ID
                      GROUP BY M.M_NAME
                    union all
                    select M.M_NAME AS poster, count(*) as postcount
                      from {_config.Value.forumTablePrefix}A_TOPICS
	                                  JOIN {_config.Value.memberTablePrefix}MEMBERS M ON T_AUTHOR = M.MEMBER_ID
                                GROUP BY M.M_NAME
                                
                       ) as u GROUP BY u.poster ORDER BY 'Value' Desc";

            if (_context.Database.IsMySql())
            {
                postsbyMember = postsbyMember + " LIMIT 10";
            }
            var data = _context.Database.SqlQueryRaw<Pair<string, Int32>>(postsbyMember);

            List<object> iData = new List<object>
            {
                data.Select(m => m.Key).ToList(), data.Select(m => m.Value).ToList()
            };

            return Json(iData);
        }
        //[HttpPost]
        public JsonResult RepliesByUser()
        {
            var sqlTop = "";
            if (_context.Database.IsSqlServer())
            {
                sqlTop = "TOP 10";
            }
            var postsbyMember = $@"select {sqlTop} poster as 'Key', CAST(sum(postcount) as INT) as 'Value' 
                              from (
                                select M.M_NAME AS poster,  count(*) as postcount
                                  from {_config.Value.forumTablePrefix}REPLY
	                                JOIN {_config.Value.memberTablePrefix}MEMBERS M ON R_AUTHOR = M.MEMBER_ID
                                    GROUP BY M.M_NAME
                                union all
                                select M.M_NAME AS poster, count(*) as postcount
                                  from {_config.Value.forumTablePrefix}A_REPLY
	                                              JOIN {_config.Value.memberTablePrefix}MEMBERS M ON R_AUTHOR = M.MEMBER_ID
                                            GROUP BY M.M_NAME
                                            
                                   ) as u GROUP BY u.poster ORDER BY 'Value' Desc";

            if (_context.Database.IsMySql())
            {
                postsbyMember = postsbyMember + " LIMIT 10";
            }
            var data = _context.Database.SqlQueryRaw<Pair<string, Int32>>(String.Format(postsbyMember,_config.Value.forumTablePrefix,_config.Value.memberTablePrefix));
            List<object> iData = new List<object>
            {
                data.Select(m => m.Key).ToList(), data.Select(m => m.Value).ToList()
            };

            return Json(iData);
        }

        public JsonResult PostsByUser()
        {
            var sqlTop = "";
            if (_context.Database.IsSqlServer())
            {
                sqlTop = "TOP 10";
            }
            var postsbyMember = $@"select {sqlTop} poster as 'Key', CAST(sum(postcount) as INT) as 'Value' 
                              from (
                                select M.M_NAME AS poster,  count(REPLY_ID) as postcount
                                  from {_config.Value.forumTablePrefix}REPLY
	                                JOIN {_config.Value.memberTablePrefix}MEMBERS M ON R_AUTHOR = M.MEMBER_ID
                                    GROUP BY M.M_NAME
                                union all
                                select M.M_NAME AS poster, count(REPLY_ID) as postcount
                                  from {_config.Value.forumTablePrefix}A_REPLY
	                                              JOIN {_config.Value.memberTablePrefix}MEMBERS M ON R_AUTHOR = M.MEMBER_ID
                                            GROUP BY M.M_NAME
                                union all
                                select M.M_NAME AS poster, count(TOPIC_ID) as postcount
                                  from {_config.Value.forumTablePrefix}TOPICS
	                              JOIN {_config.Value.memberTablePrefix}MEMBERS M ON T_AUTHOR = M.MEMBER_ID
                                  GROUP BY M.M_NAME
                                union all
                                select M.M_NAME AS poster, count(TOPIC_ID) as postcount
                                  from {_config.Value.forumTablePrefix  }A_TOPICS
	                                              JOIN {_config.Value.memberTablePrefix}MEMBERS M ON T_AUTHOR = M.MEMBER_ID
                                            GROUP BY M.M_NAME                             
                                   ) as u GROUP BY u.poster ORDER BY 'Value' Desc";

            if (_context.Database.IsMySql())
            {
                postsbyMember = postsbyMember + " LIMIT 10";
            }
            try
            {
                var data = _context.Database.SqlQueryRaw<Pair<string, Int32>>(String.Format(postsbyMember,_config.Value.forumTablePrefix,_config.Value.memberTablePrefix));
                List<object> iData = new List<object>
                {
                    data.Select(m => m.Key).ToList(), data.Select(m => m.Value).ToList()
                };

                return Json(iData);

            }
            catch (Exception)
            {

                throw;
            }
        }

        public IActionResult OnlineUsers()
        {
            //var visitors = _context.VisitorLog
            //    .OrderBy(e => e.UserName).ThenByDescending(e=>e.VisitTime)
            //    .Where(v=>v.VisitTime.DayOfYear == DateTime.UtcNow.DayOfYear)
            //    .Take(100)
            //    .ToList();
            if(!showVisits)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        public  IActionResult ClearVisitLog()
        {
            try
            {
                _context.Database.ExecuteSqlRaw($"TRUNCATE TABLE VisitorLog");
                return Ok();
            }
            catch (Exception e)
            {

                throw;
            }

        }

        public async Task<IActionResult> GetData()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = int.Parse(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = int.Parse(Request.Form["length"].FirstOrDefault() ?? "100");
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault() ?? "asc";
            var sortColumn = Request.Form[$"columns[{Request.Form["order[0][column]"]}][data]"].FirstOrDefault() ?? "visittime";

            var query = _context.VisitorLog.AsQueryable();
            // Sorting
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortDirection))
            {
                query = sortColumn.ToLower() switch
                    {
                        "username" => sortDirection.ToLower() == "asc" ? query.OrderBy(e => e.UserName).ThenByDescending(e=>e.VisitTime) : query.OrderByDescending(e => e.UserName).ThenByDescending(e=>e.VisitTime),
                        "visittime" => sortDirection.ToLower() == "asc" ? query.OrderBy(e => e.VisitTime) : query.OrderByDescending(e => e.VisitTime),
                        _ => query // Default: no sorting
                    };
            }

            var totalRecords = await query.CountAsync();

            // Fetch paginated data
            var data = new List<VisitorLog>();
            if(length != -1)
            {
                data = await query.Skip(start).Take(length).ToListAsync();
            }else{
                data = await query.ToListAsync();
            }

            // Return data in Datatables format
            return Json(new
            {
                draw = draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = data
            });
        }

        [HttpGet]
        public JsonResult VisitorLogByHour()
        {
            var today = DateTime.UtcNow.Date;
            var data = _context.VisitorLog
                .Where(v => v.VisitTime >= today)
                .GroupBy(v => v.VisitTime.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderBy(g => g.Hour)
                .ToList();

            // Ensure all 24 hours are present
            var result = Enumerable.Range(0, 24)
                .Select(h => new {
                    Hour = h,
                    Count = data.FirstOrDefault(d => d.Hour == h)?.Count ?? 0
                }).ToList();

            return Json(result);
        }
        // 1. Visits Over Time (per day)
        [HttpGet]
        public IActionResult VisitorLogByDay()
        {
            var data = _context.VisitorLog
                .GroupBy(v => v.VisitTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(g => g.Date)
                .ToList();

            return Json(data);
        }

        // 2. Top Visited Paths
        [HttpGet]
        public IActionResult TopVisitedPaths()
        {
            var data = _context.VisitorLog
                .GroupBy(v => v.Path)
                .Select(g => new { Path = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .Take(10)
                .ToList();

            return Json(data);
        }

        // 3. Unique Visitors Per Day (by UserName or IpAddress)
        [HttpGet]
        public IActionResult UniqueVisitorsPerDay()
        {
            var data = _context.VisitorLog
                .GroupBy(v => v.VisitTime.Date)
                .Select(g => new {
                    Date = g.Key,
                    UniqueCount = g.Select(x => x.UserName).Distinct().Count()
                })
                .OrderBy(x => x.Date)
                .ToList();

            return Json(data);
        }

        // 4. Visits by User Agent
        [HttpGet]
        public IActionResult VisitsByUser()
        {
            var data = _context.VisitorLog
                .GroupBy(v => v.UserName)
                .Select(g => new { UserName = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(1000)
                .ToList();

            // Step 2: Prepare pie chart data format
            var pieChartData = new
            {
                labels = data.Select(x => x.UserName).ToList(),
                datasets = new[]
                {
                    new {
                        data = data.Select(x => x.Count).ToList(),
                        backgroundColor = new[] {
                            "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF",
                            "#FF9F40", "#C9CBCF", "#FF6384", "#36A2EB", "#FFCE56"
                        }
                    }
                }
            };

            // Step 3: Return as JSON for chart rendering on client
            return Json(pieChartData);
        }

        // 6. Visits by Hour of Day (across all days)
        [HttpGet]
        public IActionResult VisitsByHour()
        {
            var data = _context.VisitorLog
                .GroupBy(v => v.VisitTime.Hour)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .OrderBy(g => g.Hour)
                .ToList();

            // Ensure all 24 hours are present
            var result = Enumerable.Range(0, 24)
                .Select(h => new {
                    Hour = h,
                    Count = data.FirstOrDefault(d => d.Hour == h)?.Count ?? 0
                }).ToList();

            return Json(result);
        }
    }
public class DataTableAjaxPostModel
{
    // properties are not capital due to json mapping
    public int draw { get; set; }
    public int start { get; set; }
    public int length { get; set; }
    public List<Column> columns { get; set; }
    public Search search { get; set; }
    public List<Order> order { get; set; }
}
    public class ChartModel
    {
        public string MinYear { get; set; }
        public string MaxYear { get; set; }
    }
    public class PostsByDate
    {
        public string y;
        public string m;
        public int tally;
    }
}
