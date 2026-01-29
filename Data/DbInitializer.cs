using PrivateTube.Models;
using PrivateTube.Services;

namespace PrivateTube.Data
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context, AuthService authService)
        {
            context.Database.EnsureCreated();

            // Seed Admin User
            if (!context.Users.Any())
            {
                var admin = new User
                {
                    Username = "admin",
                    PasswordHash = authService.HashPassword("admin123"), // Default password
                    Role = "Admin",
                    IsActive = true
                };
                context.Users.Add(admin);
                context.SaveChanges();
            }

            // Seed Sample Data
            var samplePlaylist = context.Playlists.FirstOrDefault(p => p.Name == "Sample Playlist");
            if (samplePlaylist == null)
            {
                samplePlaylist = new Playlist { Name = "Sample Playlist", Description = "Default curated videos" };
                context.Playlists.Add(samplePlaylist);
                context.SaveChanges();
            }

            if (!context.Videos.Any(v => v.PlaylistId == samplePlaylist.Id))
            {
                var videos = new[]
                {
                    new Video { Title = "Big Buck Bunny", YouTubeVideoId = "aqz-KE-bpKQ", PlaylistId = samplePlaylist.Id, IsActive = true },
                    new Video { Title = "Elephant's Dream", YouTubeVideoId = "TLkA0RELQ1g", PlaylistId = samplePlaylist.Id, IsActive = true },
                    new Video { Title = "Sintel", YouTubeVideoId = "0wCC3aLXdOw", PlaylistId = samplePlaylist.Id, IsActive = true }
                };
                context.Videos.AddRange(videos);
                context.SaveChanges();
            }

            // Assign to Admin
            var adminUserForAccess = context.Users.FirstOrDefault(u => u.Username == "admin");
            if (adminUserForAccess != null && !context.PlaylistAccesses.Any(pa => pa.UserId == adminUserForAccess.Id && pa.PlaylistId == samplePlaylist.Id))
            {
                context.PlaylistAccesses.Add(new PlaylistAccess { UserId = adminUserForAccess.Id, PlaylistId = samplePlaylist.Id });
                context.SaveChanges();
            }
        }
    }
}
