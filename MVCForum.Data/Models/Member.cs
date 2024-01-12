using Microsoft.VisualBasic.FileIO;
using SnitzCore.Data.Extensions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SnitzCore.Data.Models;

[SnitzTable("MEMBERS","MEMBER")]
public partial class Member
{
    [Key]
    [Column("MEMBER_ID")]
    [ProfileDisplay("Member Id",Order=9999,ReadOnly = true,SystemField = true)]
    public int Id { get; set; }
    [Column("M_NAME")]
    [StringLength(75)]
    [ProfileDisplay("Username",Order=9999,ReadOnly = true,SystemField = true)]
    public string Name { get; set; } = null!;

    [Column("M_EMAIL")]
    [ProfileDisplay("Email",Order=9999,ReadOnly = true,SystemField = true)]
    [StringLength(50)]
    public string Email { get; set; } = null!;
    [Column("M_TITLE")]
    [ProfileDisplay("Title",ReadOnly = true, SystemField = true)]
    [StringLength(50)]
    public string? Title { get; set; }

    [Column("M_LEVEL")]
    [ProfileDisplay("Member Level",Order=9999,SystemField = true)]
    public short Level { get; set; }
    [Column("M_STATUS")]
    [ProfileDisplay("Status",Order=9999,SystemField = true)]
    public short Status { get; set; }
    
    [Column("M_POSTS")]
    [ProfileDisplay("Posts made",SystemField = true)]
    public int Posts { get; set; }

    [Column("M_LASTHEREDATE")]
    [StringLength(14)]
    [ProfileDisplay("Last logged in",SystemField = true)]
    public string? Lastheredate { get; set; }

    [Column("M_LASTPOSTDATE")]
    [StringLength(14)]
    [ProfileDisplay("Last post",SystemField = true)]
    public string? Lastpostdate { get; set; }

    [Column("M_DATE")]
    [StringLength(14)]
    [ProfileDisplay("Member since",SystemField = true)]
    public string Created { get; set; } = null!;

    #region  Public

    [Column("M_FIRSTNAME")]
    [ProfileDisplay("Firstname",DisplayCheck = "STRFULLNAME",RequiredCheck="STRREQFULLNAME",Order = 2,LayoutSection = MemberLayout.Profile)]
    [StringLength(100)]
    public string? Firstname { get; set; }

    [Column("M_LASTNAME")]
    [ProfileDisplay("Lastname",DisplayCheck = "STRFULLNAME",RequiredCheck="STRREQFULLNAME",Order = 3,LayoutSection = MemberLayout.Profile)]
    [StringLength(100)]
    public string? Lastname { get; set; }

    [Column("M_CITY")]
    [ProfileDisplay("City",DisplayCheck = "STRCITY",RequiredCheck="STRREQCITY",Order = 4,LayoutSection = MemberLayout.Profile)]
    [StringLength(100)]
    public string? City { get; set; }

    [Column("M_STATE")]
    [ProfileDisplay("State",DisplayCheck = "STRSTATE",RequiredCheck="STRREQSTATE",Order = 5,LayoutSection = MemberLayout.Profile)]
    [StringLength(100)]
    public string? State { get; set; }

    [Column("M_PHOTO_URL")]
    [ProfileDisplay("Avatar", FieldType = "file",DisplayCheck = "STRPICTURE",RequiredCheck="STRREQPICTURE",Order = 8,LayoutSection = MemberLayout.Profile)]
    [StringLength(255)]
    public string? PhotoUrl { get; set; }
    [Column("M_COUNTRY")]
    [ProfileDisplay("Country",DisplayCheck = "STRCOUNTRY",RequiredCheck="STRREQCOUNTRY",Order = 6,LayoutSection = MemberLayout.Profile)]
    [StringLength(50)]
    public string? Country { get; set; }
    [Column("M_SEX")]
    [ProfileDisplay("Gender",SystemField = true,Order = 9999,LayoutSection = MemberLayout.Profile)]
    [StringLength(50)]
    public string? Sex { get; set; }

    [Column("M_AGE")]
    [ProfileDisplay("Age",DisplayCheck = "STRAGE",RequiredCheck="STRREQAGE",Order = 10,LayoutSection = MemberLayout.Profile)]
    [StringLength(10)]
    public string? Age { get; set; }

    [Column("M_DOB")]
    [StringLength(8)]
    [ProfileDisplay("Date of Birth",Private = true, FieldType = "datepicker",DisplayCheck = "STRAGEDOB",RequiredCheck="STRREQAGEDOB",Order = 11)]
    public string? Dob { get; set; }

    [Column("M_OCCUPATION")]
    [ProfileDisplay("Occupation",DisplayCheck = "STROCCUPATION",RequiredCheck="STRREQOCCUPAION",Order = 12,LayoutSection = MemberLayout.Bio)]
    [StringLength(255)]
    public string? Occupation { get; set; }
    [Column("M_HOMEPAGE")]
    [ProfileDisplay("Homepage",DisplayCheck = "STRHOMEPAGE",RequiredCheck="STRREQHOMEPAGE",Order = 13,LayoutSection = MemberLayout.Bio)]
    [StringLength(255)]
    public string? Homepage { get; set; }
    [Column("M_MARSTATUS")]
    [StringLength(100)]
    [ProfileDisplay("Marital Status",SystemField = true,Order = 9999,LayoutSection = MemberLayout.Profile)]
    public string? Marstatus { get; set; }
    [Column("M_DEFAULT_VIEW")]
    [ProfileDisplay("Forum Default View",SystemField = true, Order = 15,FieldType = "select",SelectEnum = "SnitzCore.Data.Models.DefaultDays",LayoutSection = MemberLayout.Extra)]
    public int DefaultView { get; set; }
    [Column("M_SIG")]
    [ProfileDisplay("Signature",FieldType = "textarea",Order=98,LayoutSection = MemberLayout.Bio)]
    public string? Signature { get; set; }
    #endregion


    #region Checkboxes
    [Column("M_VIEW_SIG")]
    [ProfileDisplay("View Signatures in posts",Order = 21, FieldType = "checkbox")]
    public short ViewSig { get; set; }

    [Column("M_SIG_DEFAULT")]
    [ProfileDisplay("Use Signature when posting",Order = 22, FieldType = "checkbox")]
    public short SigDefault { get; set; }

    [Column("M_HIDE_EMAIL")]
    [ProfileDisplay("Hide email from Members",Order = 23, FieldType = "checkbox")]
    public short HideEmail { get; set; }

    [Column("M_RECEIVE_EMAIL")]
    [ProfileDisplay("Allow Members to send you emails", Order = 24, FieldType = "checkbox")]
    public short ReceiveEmail { get; set; }
    [Column("M_PMEMAIL")]
    [ProfileDisplay("Receive email notification for Private Messages", Order = 25, FieldType = "checkbox")]
    public int Pmemail { get; set; }

    [Column("M_PMRECEIVE")]
    [ProfileDisplay("Allow Members to send you Private Messages", Order = 26, FieldType = "checkbox")]
    public int Pmreceive { get; set; }

    [Column("M_PMSAVESENT")]
    [ProfileDisplay("Save sent Private Messages", Order = 27, FieldType = "checkbox")]
    public short Pmsavesent { get; set; }

    [Column("M_PRIVATEPROFILE")]
    [ProfileDisplay("Make Profile private", Order = 28, FieldType = "checkbox")]
    public short Privateprofile { get; set; }


    #endregion

    #region System
    [Column("M_LAST_IP")]
        [StringLength(50)]
        [ProfileDisplay("Last IP address",SystemField = true)]
        public string? LastIp { get; set; }

        [Column("M_IP")]
        [StringLength(50)]
        [ProfileDisplay("Current IP address",SystemField = true)]
        public string? Ip { get; set; }
        [Column("M_ALLOWEMAIL")]
        [ProfileDisplay("Allow Emails",SystemField = true,Order=29)]
        public short Allowemail { get; set; }
        [Column("M_SUBSCRIPTION")]
        [ProfileDisplay("Allow subsriptions",Order = 29,SystemField = true)]
        public short Subscription { get; set; }
        [Column("M_KEY")]
        [ProfileDisplay("Key",SystemField = true,Order=9999)]
        [StringLength(32)]
        public string? Key { get; set; }

        [Column("M_NEWEMAIL")]
        [ProfileDisplay("Password Key",SystemField = true,Order=9999)]
        [StringLength(50)]
        public string? Newemail { get; set; }

        [Column("M_PWKEY")]
        [ProfileDisplay("Password Key",SystemField = true,Order=9999)]
        [StringLength(32)]
        public string? Pwkey { get; set; }

        [Column("M_SHA256")]
        [ProfileDisplay("Sha256",SystemField = true,Order=9999)]
        public short Sha256 { get; set; }

        [Column("M_LASTACTIVITY")]
        [StringLength(20)]
        [ProfileDisplay("Last Activity",SystemField = true,Order=9999)]
        public string? Lastactivity { get; set; }
    #endregion

    #region SocialMedia
    [Column("M_AIM")]
    [StringLength(150)]
    [ProfileDisplay("AIM",DisplayCheck = "STRAIM",RequiredCheck= "STRREQAIM", SocialLink = true,LayoutSection = MemberLayout.SocialMedia)]
    public string? Aim { get; set; }

    [Column("M_ICQ")]
    [ProfileDisplay("ICQ",DisplayCheck = "STRICQ",RequiredCheck="STRREQICQ", SocialLink = true,LayoutSection = MemberLayout.SocialMedia)]
    [StringLength(150)]
    public string? Icq { get; set; }

    [Column("M_MSN")]
    [ProfileDisplay("MSN",DisplayCheck = "STRMSN",RequiredCheck="STRREQMSN", SocialLink = true,LayoutSection = MemberLayout.SocialMedia)]
    [StringLength(150)]
    public string? Msn { get; set; }

    [Column("M_YAHOO")]
    [ProfileDisplay("Yahoo",DisplayCheck = "STRYAHOO",RequiredCheck="STRREQYAHOO", SocialLink = true,LayoutSection = MemberLayout.SocialMedia)]
    [StringLength(150)]
    public string? Yahoo { get; set; }
    
    #endregion
    
    #region Optional
    [Column("M_HOBBIES")]
    [ProfileDisplay("Hobbies",DisplayCheck = "STRHOBBIES",RequiredCheck="STRREQHOBBIES",Order = 99,FieldType = "textarea",LayoutSection = MemberLayout.Bio)]
    public string? Hobbies { get; set; }

    [Column("M_LNEWS")]
    [ProfileDisplay("Latest news",DisplayCheck = "STRLNEWS",RequiredCheck="STRREQLNEWS",Order = 99,FieldType = "textarea",LayoutSection = MemberLayout.Bio)]
    public string? Lnews { get; set; }

    [Column("M_QUOTE")]
    [ProfileDisplay("Favourite Quote",DisplayCheck = "STRQUOTE",RequiredCheck ="STRREQQUOTE",Order = 99,FieldType = "textarea",LayoutSection = MemberLayout.Bio)]
    public string? Quote { get; set; }

    [Column("M_BIO")]
    [ProfileDisplay("Biography",DisplayCheck = "STRBIO",RequiredCheck ="STRREQBIO",Order = 99,FieldType = "textarea",LayoutSection = MemberLayout.Bio)]
    public string? Bio { get; set; }
    
    [Column("M_LINK1")]
    [ProfileDisplay("Favourite Link 1",DisplayCheck = "STRFAVLINKS",RequiredCheck ="STRREQFAVLINKS",Order = 99,LayoutSection = MemberLayout.Bio)]
    [StringLength(255)]
    public string? Link1 { get; set; }

    [Column("M_LINK2")]
    [ProfileDisplay("Favourite Link 2",DisplayCheck = "STRFAVLINKS",RequiredCheck="STRREQFAVLINKS",Order = 99,LayoutSection = MemberLayout.Bio)]
    [StringLength(255)]
    public string? Link2 { get; set; }
    #endregion
    
    [NotMapped]
    [ProfileDisplay("Roles",Order=9999,ReadOnly = true,SystemField = true,LayoutSection = MemberLayout.Extra)]
    public List<string> Roles { get; set; } = new List<string>();

}
