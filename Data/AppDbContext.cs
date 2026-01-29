using Microsoft.EntityFrameworkCore;
using PrivateTube.Models;

namespace PrivateTube.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Video> Videos { get; set; }
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PlaylistAccess> PlaylistAccesses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PlaylistAccess>()
                .HasKey(pa => new { pa.UserId, pa.PlaylistId });

            modelBuilder.Entity<PlaylistAccess>()
                .HasOne(pa => pa.User)
                .WithMany(u => u.PlaylistAccesses)
                .HasForeignKey(pa => pa.UserId);

            modelBuilder.Entity<PlaylistAccess>()
                .HasOne(pa => pa.Playlist)
                .WithMany(p => p.PlaylistAccesses)
                .HasForeignKey(pa => pa.PlaylistId);
        }
    }
}
