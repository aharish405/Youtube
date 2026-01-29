using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrivateTube.Data;
using PrivateTube.Models;
using PrivateTube.ViewModels;
using System.Security.Claims;
using System.Linq;

namespace PrivateTube.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Initial load
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            IQueryable<Video> videoQuery = _context.Videos.Include(v => v.Playlist).Where(v => v.IsActive);
            IQueryable<Playlist> playlistQuery = _context.Playlists;

            if (!isAdmin)
            {
                var accessiblePlaylistIds = _context.PlaylistAccesses
                    .Where(pa => pa.UserId == userId)
                    .Select(pa => pa.PlaylistId);

                videoQuery = videoQuery.Where(v => accessiblePlaylistIds.Contains(v.PlaylistId));
                playlistQuery = playlistQuery.Where(p => accessiblePlaylistIds.Contains(p.Id));
            }

            var viewModel = new HomeViewModel
            {
                // Load first 20 for speed
                Videos = await videoQuery.OrderByDescending(v => v.CreatedDate).Take(20).ToListAsync(),
                Playlists = await playlistQuery.ToListAsync()
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> GetVideos(int page = 1)
        {
            int pageSize = 20;
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            IQueryable<Video> videoQuery = _context.Videos.Include(v => v.Playlist).Where(v => v.IsActive);

            if (!isAdmin)
            {
                var accessiblePlaylistIds = _context.PlaylistAccesses
                    .Where(pa => pa.UserId == userId)
                    .Select(pa => pa.PlaylistId);
                videoQuery = videoQuery.Where(v => accessiblePlaylistIds.Contains(v.PlaylistId));
            }

            var videos = await videoQuery
                .OrderByDescending(v => v.CreatedDate)
                .Skip(page * pageSize)
                .Take(pageSize)
                .Select(v => new
                {
                    v.Id,
                    v.Title,
                    v.YouTubeVideoId,
                    v.CreatedDate,
                    PlaylistName = v.Playlist.Name,
                    pHash = v.PlaylistId.GetHashCode()
                })
                .ToListAsync();

            return Json(videos);
        }

        public async Task<IActionResult> Search(string query)
        {
            ViewData["Query"] = query;
            if (string.IsNullOrWhiteSpace(query)) return RedirectToAction("Index");

            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            IQueryable<Video> videoQuery = _context.Videos.Include(v => v.Playlist)
                .Where(v => v.IsActive && v.Title.Contains(query));

            if (!isAdmin)
            {
                var accessiblePlaylistIds = _context.PlaylistAccesses
                    .Where(pa => pa.UserId == userId)
                    .Select(pa => pa.PlaylistId);

                videoQuery = videoQuery.Where(v => accessiblePlaylistIds.Contains(v.PlaylistId));
            }

            var viewModel = new HomeViewModel
            {
                Videos = await videoQuery.ToListAsync(),
                Playlists = new List<Playlist>() // Empty for search view usually
            };

            return View("Index", viewModel);
        }

        public async Task<IActionResult> Playlist()
        {
            // Simplified Playlist View logic if needed separately, otherwise Index handles "All". 
            // Keeping it consistent with "All Playlists" view if requested, or redirect to Index.
            // For now, let's just show the Playlists view from previous step but adapting to new Layout.
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            var playlists = isAdmin
                ? await _context.Playlists.Include(p => p.Videos).ToListAsync()
                : await _context.Playlists.Where(p => p.PlaylistAccesses.Any(pa => pa.UserId == userId)).Include(p => p.Videos).ToListAsync();

            // Reuse Index view or create separate? Let's use Index but only show Playlists roughly? 
            // Actually user asked for "show all videos on landing page". 
            // Let's create a View for "Browse Playlists" if they click "Playlists" in sidebar.
            return View(playlists);
        }

        public async Task<IActionResult> Watch(int id)
        {
            var video = await _context.Videos.Include(v => v.Playlist).FirstOrDefaultAsync(v => v.Id == id);
            if (video == null) return NotFound();

            // Check Access
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin)
            {
                var hasAccess = await _context.PlaylistAccesses
                    .AnyAsync(pa => pa.UserId == userId && pa.PlaylistId == video.PlaylistId);
                if (!hasAccess) return Forbid();
            }

            // Recommendations System
            // 1. Get other videos from same playlist
            // 2. Get some random other videos (if needed to fill up)
            // 3. Find Next Video

            IQueryable<Video> baseQuery = _context.Videos.Include(v => v.Playlist).Where(v => v.IsActive && v.Id != id);

            if (!isAdmin)
            {
                var accessiblePlaylistIds = _context.PlaylistAccesses
                     .Where(pa => pa.UserId == userId)
                     .Select(pa => pa.PlaylistId);
                baseQuery = baseQuery.Where(v => accessiblePlaylistIds.Contains(v.PlaylistId));
            }

            var recommended = await baseQuery
                .OrderByDescending(v => v.PlaylistId == video.PlaylistId) // Prioritize same playlist
                .ThenBy(v => Guid.NewGuid()) // Randomize rest
                .Take(10)
                .ToListAsync();

            var nextVideo = recommended.FirstOrDefault(v => v.PlaylistId == video.PlaylistId && v.Id > id)
                            ?? recommended.FirstOrDefault(v => v.PlaylistId == video.PlaylistId)
                            ?? recommended.FirstOrDefault();

            var viewModel = new WatchViewModel
            {
                CurrentVideo = video,
                RecommendedVideos = recommended,
                NextVideo = nextVideo
            };

            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult Error()
        {
            return View();
        }
    }
}
