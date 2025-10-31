using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Models;
using System.Runtime.CompilerServices;

namespace SnitzCore.BackOffice.Controllers
{
    [Authorize(Roles = "Administrator, Moderator")]
    public class ChartsController : Controller
    {
        private readonly SnitzDbContext _context;
        private readonly IOptions<SnitzForums> _config;
        public ChartsController(SnitzDbContext context,IOptions<SnitzForums> config)
        {
            _context = context;
            _config = config;
        }
        public IActionResult Index()
        {
            var yearQuery = @"select min(SUBSTRING(T_DATE, 1, 4)) as MinYear, max(SUBSTRING(T_DATE, 1, 4)) as MaxYear FROM " + _config.Value.forumTablePrefix + "TOPICS";
            FormattableString formattableQuery = FormattableStringFactory.Create(yearQuery);
            ChartModel? Years = _context.Database.SqlQuery<ChartModel>(formattableQuery).FirstOrDefault();
            return View(Years);

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
                Console.WriteLine(e);
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
