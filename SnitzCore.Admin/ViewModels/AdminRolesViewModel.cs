using SnitzCore.Data;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;


namespace SnitzCore.BackOffice.ViewModels
{
    //public enum ForumType
    //{
    //    Topics = 0,
    //    WebLink = 1,
    //    //[Description("Bug Forum")]
    //    BugReports = 3,
    //    BlogPosts = 4
    //}
    public class AdminRolesViewModel
    {
        //[RequiredIf("IsUsernameRequired", "true", "PropertyRequired")]
        public string? Username { get; set; }
        //[RequiredIf("IsUsernameRequired", "true", "PropertyRequired")]
        public string? Rolename { get; set; }

        //[RequiredIf("IsRolenameRequired", "true", "PropertyRequired")]
        public string? NewRolename { get; set; }

        public List<Member>? Members { get; set; }

        public string?[]? RoleList { get; set; }

        public bool IsUsernameRequired { get; set; }
        public bool IsRolenameRequired { get; set; }
    }
    public enum RankType
    {
        None = 0,
        RankOnly,
        StarsOnly,
        Both
    }
    public class RankingViewModel
    {
        private readonly SnitzDbContext _context;
        private readonly ISnitzConfig _config;
        public RankingViewModel(SnitzDbContext dbContext,ISnitzConfig config)
        {
            _context = dbContext;
            _config = config;
        }
        public Dictionary<int, MemberRanking> Ranks
        {
            get { return _context.MemberRanking.ToDictionary(r=>r.Id,r=>r); }
        }

        public RankType Type
        {
            get { return (RankType)_config.GetIntValue("STRSHOWRANK"); }
        }
    }
    public class RankingPost
    {
        public RankType Type {get;set;}
        public Dictionary<int, MemberRanking> Ranks  {get;set;}
    }
    public class AdminGroupsViewModel
    {

        public int GroupNameId { get; set; }
        public string? GroupName { get; set; }
        public List<GroupName>? Groups { get; set; }
        public List<Category> CategoryList { get; set; }
        public Dictionary<int, string>? Categories { get; set; }

        public AdminGroupsViewModel(int id,ICategory category)
        {
            this.GroupNameId = id;
            CategoryList = category.GetAll().ToList();
            if (id > 0)
            {
                this.Categories = new Dictionary<int, string>();
                foreach (var (key, value) in category.GetGroups().Where(g=>g.GroupNameId == id).ToDictionary(t => t.CategoryId, t => t.Category!.Name))
                {
                    if (value != null) this.Categories.Add(key, value);
                }
            }
            else
            {
                this.Categories = null;
            }
        }
    }
    //public class AdminSubscriptionsViewModel
    //{
    //    public List<Subscriptions> Subscriptions { get; set; }

    //    public Enumerators.SubscriptionLevel SubscriptionLevel { get; set; }
    //    public Enumerators.SubscriptionLevel VisibleLevel { get; set; }
    //}

    public class AdminEmailServer
    {
        [Required]
        public string ContactEmail { get; set; } = null!;

        [Required]
        public string? Server { get; set; }
        public bool Auth { get; set; }
        public int Port { get; set; }
        public string? Username { get; set; }
        [PasswordPropertyText]
        public string? Password { get; set; }
        [Required]
        public string From { get; set; } = null!;

        public SmtpDeliveryMethod DeliveryMethod { get; set; }
        public string? PickUpFolder { get; set; }
        public bool DefaultCred { get; set; }

        public string? UseSpamFilter { get; set; }
        public string? EmailDomain { get; set; }
        public SpamFilter[]? BannedDomains { get; set; }
        public string? EmailMode { get; set; }

        public string? SslMode { get; set; }
    }

    //public class AdminFeaturesViewModel
    //{
    //    public List<Enumerators.CaptchaOperator> CaptchaOperators { get { return SnitzConfig.Config.CaptchaOperators ?? new List<Enumerators.CaptchaOperator>() { Enumerators.CaptchaOperator.Plus }; } }
    //    public Dictionary<string, string> Config
    //    {
    //        get
    //        {
    //            return ClassicConfig.ConfigDictionary;
    //        }
    //    }
    //    public Enumerators.SubscriptionLevel SubscriptionLevel { get; set; }
    //    public Dictionary<int,string> AllowedForums { get; set; }

    //    public Dictionary<int, string> ForumList
    //    {
    //        get
    //        {
    //            List<int> allowed = new List<int>(ClassicConfig.GetValue("STRAPIFORUMS").StringToIntList());
    //            this.AllowedForums = new Dictionary<int, string>();

    //            var forumList = new Dictionary<int, string>();
    //            var forums = Forum.List(HttpContext.Current.User);
    //            foreach (KeyValuePair<int, string> forum in forums.ToDictionary(t => t.Key, t => t.Value))
    //            {
    //                forumList.Add(forum.Key, forum.Value);
    //                if (allowed.Contains(forum.Key))
    //                {
    //                    this.AllowedForums.Add(forum.Key, forum.Value);
    //                }
    //            }
    //            return forumList;
    //        }
    //    }

    //    public object ForumId { get; set; }

    //    public string GetValue(string key, string def="")
    //    {
    //        if (Config.ContainsKey(key))
    //        {
    //            return Config[key];
    //        }
    //        return def;
    //    }
    //}
}