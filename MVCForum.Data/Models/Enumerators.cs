using System.ComponentModel.DataAnnotations;

namespace SnitzCore.Data.Models
{
    public enum DefaultDays
    {
        [Display(Name = "Show all open topics")]
        AllOpen = -1,
        [Display(Name = "Show all topics")]
        All=0, 
        [Display(Name = "Show topics from last day")]
        LastDay,
        [Display(Name = "Show topics from last 2 days")]
        Last2Days,
        [Display(Name = "Show topics from last 5 days")]
        Last5Days=5,
        [Display(Name = "Show topics from last week")]
        Last7Days=7,
        [Display(Name = "Show topics from last 2 weeks")]
        Last14Days=14,
        [Display(Name = "Show topics from last 30 days")]
        Last30Days=30,
        [Display(Name = "Show topics from last 60 days")]
        Last60Days=60,
        [Display(Name = "Show topics from last 120 days")]
        Last120Days=120,
        [Display(Name = "Show topics from last year")]
        LastYear=365,
        //[Display(Name = "Show Draft Po")]
        //Draft= -9999,
        [Display(Name = "Show archived topics")]
        Archived=-99,
        [Display(Name = "Unanswered Posts")]
        NoReplies=-999,
        [Display(Name = "Only Hot Topics")]
        Hot=-88,

    }

    public enum ForumAuthType
    {
        All = 0,
        [Display(Name = "Allowed Members")]
        AllowedMembers = 1,
        [Display(Name = "Password Protected")]
        PasswordProtected,
        [Display(Name = "Allowed Member Password")]
        AllowedMemberPassword,
        [Display(Name = "Members Only")]
        Members,
        [Display(Name = "Members Only (Hidden)")]
        MembersHidden,
        [Display(Name = "Allowed Members (Hidden)")]
        AllowedMembersHidden,
        [Display(Name = "Members Password")]
        MembersPassword
    }
    /// <summary>
    /// Forum types
    /// </summary>
    public enum ForumType
    {
        [Display(Name = "Standard Forum")]
        Topics = 0,
        [Display(Name = "Url Link")]
        WebLink = 1,
        [Display(Name = "Bug Forum")]
        BugReports = 3,
        BlogPosts = 4
    }

    public enum PostType
    {
        Topic,
        Reply
    }
    public enum ActiveSince
    {
        [Display(Name="Last Visit ")]
        LastVisit,
        [Display(Name="Last 15 minutes")]
        LastFifteen,
        [Display(Name="Last 30 minutes")]
        LastThirty,
        [Display(Name="Last hour")]
        LastHour,
        [Display(Name="Last 2 hours")]
        Last2Hours,
        [Display(Name="Last 6 hours")]
        Last6Hours,
        [Display(Name="Last 12 hours")]
        Last12Hours,
        [Display(Name="Yesterday")]
        LastDay,
        [Display(Name="Last 2 days")]
        Last2Days,
        [Display(Name="Last week")]
        LastWeek,
        [Display(Name="Last 2 weeks")]
        Last2Weeks,
        [Display(Name="Last month")]
        LastMonth,
        [Display(Name="Last 2 months")]
        Last2Months,
    }

    public enum ActiveRefresh
    {
        [Display(Name="Don't reload automatically")]
        None,
        [Display(Name="Reload page every minute")]
        Minute,
        [Display(Name="Reload page every 2 minutes")]
        TwoMinute,
        [Display(Name="Reload page every 5 minutes")]
        FiveMinute=5,
        [Display(Name="Reload page every 10 minutes")]
        TenMinute=10,
        [Display(Name="Reload page every 15 minutes")]
        FifteenMinute=15,
    }

    public enum SearchFor
    {
        ExactPhrase,
        AllTerms,
        AnyTerms
    }

    public enum SearchDate
    {
        [Display(Name="Any Date")]
        AnyDate = 0,
        [Display(Name="Since Yesterday")]
        Since1Day,
        [Display(Name="In the last 2 days")]
        Since2Days,
        [Display(Name="In the last 5 days")]
        Since5Days = 5,
        [Display(Name="In the last week")]
        Since7Days = 7,
        [Display(Name="In the last 2 weeks")]
        Since14Days = 14,
        [Display(Name="In the last month")]
        Since1Month = 30,
        [Display(Name="In the last 2 months")]
        Since2Months = 60,
        [Display(Name="In the last 6 months")]
        Since6Months = 184,
        [Display(Name="In the last year")]
        SinceYear = 365,

    }
}
