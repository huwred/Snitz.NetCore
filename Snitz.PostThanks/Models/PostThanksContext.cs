using Microsoft.EntityFrameworkCore;


namespace Snitz.PostThanks.Models
{
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
