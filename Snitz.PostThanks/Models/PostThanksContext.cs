using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;


namespace Snitz.PostThanks.Models
{
    //public class PostThanksContextFactory : IDesignTimeDbContextFactory<PostThanksContext>
    //{
    //    public PostThanksContext CreateDbContext(string[] args)
    //    {
    //        var path = System.IO.Path.Combine( System.Text.RegularExpressions.Regex.Replace(AppDomain.CurrentDomain.BaseDirectory, @"\\bin$", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ,  "App_Data", "SnitzForums2000.sqlite.db");

    //        var optionsBuilder = new DbContextOptionsBuilder<PostThanksContext>();
    //        optionsBuilder.UseSqlite(path);

    //        return new PostThanksContext(optionsBuilder.Options);
    //    }
    //}
    public class PostThanksContext : DbContext
    {

        public DbSet<PostThanksEntry> PostThanks { get; set; }

        public PostThanksContext(DbContextOptions<PostThanksContext> options) : base(options){} 


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Essential to call the base method!

            modelBuilder.Entity<PostThanksEntry>().HasKey(e => new { e.MemberId, e.TopicId,e.ReplyId });
        }
    }
}
