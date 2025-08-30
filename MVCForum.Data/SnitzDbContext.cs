using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SnitzCore.Data.Models;
using System;
using System.Linq;
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
            optionsBuilder.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IdentityRole>();//.HasData(roles);

            modelBuilder.Entity<IdentityUserRole<string>>();

            modelBuilder.Entity<ForumAllowedMember>().HasKey(e => new { e.MemberId, e.ForumId });;
            modelBuilder.Entity<Badword>();
            modelBuilder.Entity<Group>();
            modelBuilder.Entity<GroupName>();
            modelBuilder.Entity<ForumModerator>();
            modelBuilder.Entity<MemberNamefilter>();//.HasData(new MemberNamefilter { Id = 1, Name = "Administrator" });
            modelBuilder.Entity<Post>();
            modelBuilder.Entity<PostReply>();
            modelBuilder.Entity<MemberSubscription>();
            modelBuilder.Entity<ArchivedPost>();
            modelBuilder.Entity<ArchivedReply>().HasOne(r=>r.Topic);

            modelBuilder.Entity<Member>();
            modelBuilder.Entity<Category>();//.HasData(new Category { Id = 1, Status = 1, Sort = 0, Name = "General", Moderation = 0 });
            modelBuilder.Entity<Forum>() //.HasData(new Forum { Id = 1, CategoryId = 1, Defaultdays = 30, Status = 1, CountMemberPosts = 1, Title = "Testing Forums", Description = "This forum gives you a chance to become more familiar with how this product responds to different features and keeps testing in one place instead of posting tests all over. Happy Posting! [:)]" });
                        .HasOne(f => f.LatestTopic);
                        //.WithOne()
                        //.HasForeignKey<Post>(p => p.Id);
            modelBuilder.Entity<SnitzConfig>().HasIndex(p => new { p.Key })
            .IsUnique(true);;
            modelBuilder.Entity<PrivateMessage>();
            modelBuilder.Entity<PrivateMessageBlocklist>();
            modelBuilder.Entity<BookmarkEntry>();
            modelBuilder.Entity<OldUserInRole>().HasNoKey();
            modelBuilder.Entity<MemberRanking>()
                    .Property(x => x.Id)
                    .UseIdentityColumn(seed: 0, increment: 1);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("Snitz.")).ToArray();
            foreach (var assembly in assemblies)
            {
                modelBuilder.ApplyConfigurationsFromAssembly(assembly);
            }

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
        public DbSet<MemberSubscription> MemberSubscriptions { get; set; }
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
