using System.ComponentModel.DataAnnotations;

namespace SnitzCore.Data.Extensions
{
    public enum ActiveSince
    {
        [Display(Name="ActiveTopicsSince_LastVisit")]
        LastVisit,
        [Display(Name="ActiveTopicsSince_LastFifteen")]
        LastFifteen,
        [Display(Name="ActiveTopicsSince_LastThirty")]
        LastThirty,
        [Display(Name="ActiveTopicsSince_Lasthour")]
        LastHour,
        [Display(Name="ActiveTopicsSince_Last2hours")]
        Last2Hours,
        [Display(Name="ActiveTopicsSince_Last6hours")]
        Last6Hours,
        [Display(Name="ActiveTopicsSince_Last12hours")]
        Last12Hours,
        [Display(Name="ActiveTopicsSince_LastDay")]
        LastDay,
        [Display(Name="ActiveTopicsSince_Last2days")]
        Last2Days,
        [Display(Name="ActiveTopicsSince_Lastweek")]
        LastWeek,
        [Display(Name="ActiveTopicsSince_Last2weeks")]
        Last2Weeks,
        [Display(Name="ActiveTopicsSince_Lastmonth")]
        LastMonth,
        [Display(Name="ActiveTopicsSince_Last2months")]
        Last2Months,
    }
}
