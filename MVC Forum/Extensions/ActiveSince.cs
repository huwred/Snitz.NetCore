using SnitzCore.Service;
using System.ComponentModel.DataAnnotations;

namespace MVCForum.Extensions
{
    public enum ActiveSince
    {
        [Display(Name="ActiveTopicsSince_LastVisit")]
        LastVisit,
        [Display(Name="ActiveTopicsSince_LastFifteen")]
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
}
