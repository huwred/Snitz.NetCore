using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SnitzCore.Data.Models;
using System;
using System.Reflection;

namespace SnitzCore.Data
{
    public class SnitzDbContext : IdentityDbContext
    {
        public SnitzDbContext(){}
        public SnitzDbContext(DbContextOptions<SnitzDbContext> options,IServiceProvider serviceProvider)
            : base(options)
        {
            //SavedChanges += SnitzContext_SavedChanges;

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //optionsBuilder.EnableSensitiveDataLogging();
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //var roles = new List<IdentityRole>
            //{
            //    new(){Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Administrator", NormalizedName = "Administrator".ToUpper() },
            //    new(){Id = "0D1F96F3-A8BD-4348-AFA4-61B931BB3553", Name = "Moderator", NormalizedName = "Moderator".ToUpper() },
            //    new(){Id = "467DF002-6D82-4109-979A-76F01FA9D4CF", Name = "ForumMember", NormalizedName = "ForumMember".ToUpper() }
            //};
            modelBuilder.Entity<IdentityRole>();//.HasData(roles);
            //Seeding the User to AspNetUsers table

            //var hasher = new PasswordHasher<IdentityUser>();
            //modelBuilder.Entity<ForumUser>().HasData(new ForumUser
            //{
            //    Id = "8e445865-a24d-4543-a6c6-9443d048cdb9",
            //    MemberId = 1,
            //    IsAdmin = true,
            //    Email = "xxxx@example.com",
            //    NormalizedEmail = "XXXX@EXAMPLE.COM",
            //    UserName = "Adminstrator",
            //    NormalizedUserName = "ADMINISTRATOR",
            //    PhoneNumber = "+111111111111",
            //    EmailConfirmed = true,
            //    PhoneNumberConfirmed = true,
            //    PasswordHash = hasher.HashPassword(null!, "Pa$$w0rd"),
            //    SecurityStamp = Guid.NewGuid().ToString("D")
            //});
            modelBuilder.Entity<IdentityUserRole<string>>();
            //.HasData(
            //    new IdentityUserRole<string>
            //    {
            //        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",
            //        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb9"
            //    }
            //);

            modelBuilder.Entity<ForumAllowedMember>().HasNoKey();
            modelBuilder.Entity<Badword>();
            modelBuilder.Entity<Group>();
            modelBuilder.Entity<GroupName>();
            modelBuilder.Entity<ForumModerator>();
            modelBuilder.Entity<MemberNamefilter>();//.HasData(new MemberNamefilter { Id = 1, Name = "Administrator" });
            modelBuilder.Entity<Post>();
            modelBuilder.Entity<PostReply>();
            modelBuilder.Entity<MemberSubscription>();
            modelBuilder.Entity<ArchivedPost>();
            modelBuilder.Entity<ArchivedReply>();

            modelBuilder.Entity<Member>();
            //.HasData(new Member
            //{
            //    Id = 1,
            //    Status = 1,
            //    Name = "Administrator",
            //    Level = 3,
            //    Email = "xxxx@example.com",
            //    Created = DateTime.UtcNow.ToForumDateStr()

            //});
            modelBuilder.Entity<Category>();//.HasData(new Category { Id = 1, Status = 1, Sort = 0, Name = "General", Moderation = 0 });
            //modelBuilder.Entity<Category>().HasOne(d => d.Group);
            modelBuilder.Entity<Forum>();//.HasData(new Forum { Id = 1, CategoryId = 1, Defaultdays = 30, Status = 1, CountMemberPosts = 1, Title = "Testing Forums", Description = "This forum gives you a chance to become more familiar with how this product responds to different features and keeps testing in one place instead of posting tests all over. Happy Posting! [:)]" });

            modelBuilder.Entity<SnitzConfig>();

            modelBuilder.Entity<PrivateMessage>();
            modelBuilder.Entity<PrivateMessageBlocklist>();

            modelBuilder.Entity<BookmarkEntry>();
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("Snitz.PhotoAlbum"));
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("Snitz.Events"));
            modelBuilder.Entity<OldUserInRole>().HasNoKey();
        }



        public DbSet<ForumUser> ApplicationUser { get; set; }

        public DbSet<OldMembership> OldMemberships { get; set; }
        public DbSet<OldRole> OldRoles { get; set; }
        public DbSet<OldUserInRole> OldUsersInRoles { get; set; }

        public DbSet<Member> Members { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Forum> Forums { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<PostReply> Replies { get; set; }

        public DbSet<ArchivedReply> ArchivedPosts { get; set; }
        public DbSet<ArchivedPost> ArchivedTopics { get; set; }
        public DbSet<Badword> Badwords { get; set; }
        public DbSet<ForumAllowedMember> ForumAllowedMembers { get; set; }
        public DbSet<ForumModerator> ForumModerator { get; set; }
        public DbSet<ForumTotal> ForumTotal { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupName> GroupName { get; set; }
        public DbSet<MemberNamefilter> MemberNamefilter { get; set; }
        public DbSet<MemberSubscription> MemberSubscription { get; set; }
        public DbSet<SnitzConfig> SnitzConfig { get; set; }
        public DbSet<LanguageResource> LanguageResources { get; set; }
        public DbSet<SpamFilter> SpamFilter { get; set; }
        public DbSet<TopicRating> TopicRating { get; set; }
        public DbSet<Poll> Polls { get; set; }
        public DbSet<PollAnswer> PollAnswers { get; set; }
        public DbSet<PollVote> PollVotes { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }
        public DbSet<PrivateMessageBlocklist> PrivateMessagesBlocklist { get; set; }
        public DbSet<MemberRanking> MemberRanking { get; set; }

        public DbSet<BookmarkEntry> Bookmarks { get; set; }
    }
}
