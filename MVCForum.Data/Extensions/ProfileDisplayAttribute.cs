using System.ComponentModel;
using SnitzCore.Data.Models;

namespace SnitzCore.Data.Extensions
{
    public class ProfileDisplayAttribute : DisplayNameAttribute
    {
        public string? DisplayCheck { get; set; }
        public string? RequiredCheck { get; set; }
        public int Order { get; set; } = 999;
        public string FieldType { get; set; } = "text";
        public bool ReadOnly { get; set; } = false;

        public bool Private { get; set; } = false;
        public bool SystemField { get; set; } = false;
        public bool SocialLink { get; set; }

        public MemberLayout LayoutSection { get; set; }
        public ProfileDisplayAttribute(string stringName) : base(stringName) { }

        public string? SelectEnum { get; set; }
    }
}