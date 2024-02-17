using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SnitzCore.Data;
using SnitzCore.Data.Models;
using System.Configuration;

namespace Snitz.PhotoAlbum.Models
{

    public class PhotoContext : SnitzDbContext
    {
        private readonly IConfiguration _configuration;

        public DbSet<AlbumImage> AlbumImages { get; set; }
        //public DbSet<AlbumGroup> AlbumGroups { get; set; }
        //public DbSet<AlbumCategory> AlbumCategories { get; set; }
        //public DbSet<ExtendedMember> ExtendedMembers { get; set; }
        public PhotoContext(){}
        public PhotoContext(DbContextOptions<SnitzDbContext> options,IServiceProvider serviceProvider,IConfiguration configuration) : base(options,serviceProvider)
        {
            _configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-BM5D2ET;Database=SnitzCoreMVC;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true");
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Essential to call the base method!

            modelBuilder.Entity<AlbumImage>();
            modelBuilder.Entity<AlbumGroup>();
            modelBuilder.Entity<AlbumCategory>();

        }
    }
}
