using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using SnitzCore.Data;


namespace Snitz.PhotoAlbum.Models
{
    //public class PhotoContextFactory : IDesignTimeDbContextFactory<PhotoContext>
    //{
    //    public PhotoContext CreateDbContext(string[] args)
    //    {
    //        var path = System.IO.Path.Combine( System.Text.RegularExpressions.Regex.Replace(AppDomain.CurrentDomain.BaseDirectory, @"\\bin$", String.Empty, System.Text.RegularExpressions.RegexOptions.IgnoreCase) ,  "App_Data", "SnitzForums2000.sqlite.db");

    //        var optionsBuilder = new DbContextOptionsBuilder<PhotoContext>();
    //        optionsBuilder.UseSqlite(path);

    //        return new PhotoContext(optionsBuilder.Options);
    //    }
    //}
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
