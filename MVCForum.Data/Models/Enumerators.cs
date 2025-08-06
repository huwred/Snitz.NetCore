using System.ComponentModel.DataAnnotations;

namespace SnitzCore.Data.Models
{
    public enum PollAuth
    {
        Disallow,
        Members,
        AdminModerators
    }
    public enum MyTopicsSince
    {
        LastVisit = 0,
        LastHour,
        Last2Hours,
        Last6Hours,
        Last12Hours,
        LastDay,
        LastWeek,
        LastMonth,
        Last6Months,
        Last12Months
    }
    public enum MemberLayout
    {
        Profile,
        SocialMedia,
        Bio,
        Extra,
        Signature
    }
    public enum DefaultDays
    {
        [Display(Name = "ForumDays_AllOpen")]
        AllOpen = -1,
        [Display(Name = "ForumDays_All")]
        All=0, 
        [Display(Name = "ForumDays_LastDay")]
        LastDay,
        [Display(Name = "ForumDays_Last2Days")]
        Last2Days,
        [Display(Name = "ForumDays_Last5Days")]
        Last5Days=5,
        [Display(Name = "ForumDays_Last7Days")]
        Last7Days=7,
        [Display(Name = "ForumDays_Last14Days")]
        Last14Days=14,
        [Display(Name = "ForumDays_Last30Days")]
        Last30Days=30,
        [Display(Name = "ForumDays_Last60Days")]
        Last60Days=60,
        [Display(Name = "ForumDays_Last120Days")]
        Last120Days=120,
        [Display(Name = "ForumDays_LastYear")]
        LastYear=365,
        //[Display(Name = "ForumDays_Draft")]
        //Draft= -9999,
        [Display(Name = "ForumDays_Archived")]
        Archived=-99,
        [Display(Name = "ForumDays_NoReplies")]
        NoReplies=-999,
        [Display(Name = "ForumDays_Hot")]
        Hot=-88,

    }

    public enum ForumAuthType
    {
        All = 0,
        [Display(Name = "ForumAuthType_AllowedMembers")]
        AllowedMembers = 1,
        [Display(Name = "ForumAuthType_PasswordProtected")]
        PasswordProtected,
        [Display(Name = "Allowed Member Password")]
        AllowedMemberPassword,
        [Display(Name = "ForumAuthType_Members")]
        Members,
        [Display(Name = "ForumAuthType_MembersHidden")]
        MembersHidden,
        [Display(Name = "ForumAuthType_AllowedMembersHidden")]
        AllowedMembersHidden,
        [Display(Name = "ForumAuthType_MembersPassword")]
        MembersPassword
    }
    /// <summary>
    /// Forum types
    /// </summary>
    public enum ForumType
    {
        [Display(Name = "ForumType_Topics")]
        Topics = 0,
        [Display(Name = "ForumType_WebLink")]
        WebLink = 1,
        [Display(Name = "ForumType_BugReports")]
        BugReports = 3,
        BlogPosts = 4
    }
    public enum CaptchaOperator
    {
        Plus,
        Minus,
        Multiply
    }
    public enum PostType
    {
        Topic,
        Reply
    }

    public enum PostAuthType
    {
        Anyone = 0,
        Admins,
        Moderators
    }
    public enum ActiveRefresh
    {
        [Display(Name="ActiveRefresh_None")]
        None,
        [Display(Name="ActiveRefresh_Minute")]
        Minute,
        [Display(Name="ActiveRefresh_TwoMinute")]
        TwoMinute,
        [Display(Name="ActiveRefresh_FiveMinute")]
        FiveMinute=5,
        [Display(Name="ActiveRefresh_TenMinute")]
        TenMinute=10,
        [Display(Name="ActiveRefresh_FifteenMinute")]
        FifteenMinute=15,
    }

    public enum SearchFor
    {
        [Display(Name = "SearchWordMatch_ExactPhrase")]
        ExactPhrase,
        [Display(Name = "SearchWordMatch_All")]
        AllTerms,
        [Display(Name = "SearchWordMatch_Any")] 
        AnyTerms
    }
    public enum ActiveStatus
    {
        Locked,
        Active
    }
    public enum SearchDate
    {
        [Display(Name="SearchDays_Any")]
        AnyDate = 0,
        [Display(Name="SearchDays_Since1Day")]
        Since1Day = 1,
        [Display(Name="ActiveTopicsSince_Last2Days")]
        Since2Days = 2,
        [Display(Name="ActiveTopicsSince_Last5Days")]
        Since5Days = 5,
        [Display(Name="SearchDays_Since7Days")]
        Since7Days = 7,
        [Display(Name="SearchDays_Since14Days")]
        Since14Days = 14,
        [Display(Name="SearchDays_Since30Days")]
        Since1Month = 30,
        [Display(Name="SearchDays_Since60Days")]
        Since2Months = 60,
        [Display(Name="SearchDays_Since120Days")]
        Since6Months = 184,
        [Display(Name="SearchDays_SinceYear")]
        SinceYear = 365,

    }

    /// <summary>
    /// Post Status
    /// </summary>
    public enum Status
    {
        [Display(Name="PostStatus_Closed")]
        Closed = 0,
        [Display(Name="PostStatus_Open")]
        Open = 1,
        [Display(Name="PostStatus_Unmoderated")]
        UnModerated = 2,
        [Display(Name="PostStatus_OnHold")]
        OnHold = 3,
        [Display(Name="PostStatus_Draft")]
        Draft = 99
    }
    public enum Moderation
    {
        [Display(Name = "Moderation_UnModerated")]
        UnModerated = 0,
        [Display(Name = "Moderation_AllPosts")]
        AllPosts,
        [Display(Name = "Moderation_Topics")]
        Topics,
        [Display(Name = "Moderation_Replies")]
        Replies
    }
    public enum ModerationLevel
    {
        [Display(Name = "ModerationLevel_NotAllowed")]
        NotAllowed = 0,
        [Display(Name = "ModerationLevel_Allowed")]
        Allowed
    }
    public enum CategorySubscription
    {
        [Display(Name = "CategorySubscription_None")]
        None = 0,
        [Display(Name = "CategorySubscription_CategorySubscription")]
        CategorySubscription,
        [Display(Name = "CategorySubscription_ForumSubscription")]
        ForumSubscription,
        [Display(Name = "CategorySubscription_TopicSubscription")] 
        TopicSubscription
    }
    /// <summary>
    /// Allowed Subscription level
    /// </summary>
    public enum ForumSubscription
    {
        [Display(Name = "Subscription_None")]
        None = 0,
        [Display(Name = "Subscription_ForumSubscription")]
        ForumSubscription,
        [Display(Name = "Subscription_TopicSubscription")]
        TopicSubscription
    }
    /// <summary>
    /// Forum Subscription Level
    /// </summary>
    public enum SubscriptionLevel
    {
        None = 0,
        Board,
        Category,
        Forum,
        Topic
    }
}
