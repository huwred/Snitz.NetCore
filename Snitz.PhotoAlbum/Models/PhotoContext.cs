using Microsoft.EntityFrameworkCore;


namespace Snitz.PhotoAlbum.Models
{

    public class PhotoContext : DbContext
    {

        public DbSet<AlbumImage> AlbumImages { get; set; }
        //public DbSet<AlbumGroup> AlbumGroups { get; set; }
        //public DbSet<AlbumCategory> AlbumCategories { get; set; }
        //public DbSet<ExtendedMember> ExtendedMembers { get; set; }
        public PhotoContext(DbContextOptions<PhotoContext> options) : base(options){} 


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Essential to call the base method!

            modelBuilder.Entity<AlbumImage>();
            modelBuilder.Entity<AlbumGroup>();
            modelBuilder.Entity<AlbumCategory>();

        }
    }
}
