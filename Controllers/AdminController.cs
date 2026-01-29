using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrivateTube.Data;
using PrivateTube.Models;
using PrivateTube.Services;

namespace PrivateTube.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly AuthService _authService;

        public AdminController(AppDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        public IActionResult Index()
        {
            return View();
        }

        // --- User Management ---
        public async Task<IActionResult> Users()
        {
            return View(await _context.Users.Include(u => u.PlaylistAccesses).ToListAsync());
        }

        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(string username, string password, string role)
        {
            if (_context.Users.Any(u => u.Username == username))
            {
                ModelState.AddModelError("", "Username already exists");
                return View();
            }

            var user = new User
            {
                Username = username,
                PasswordHash = _authService.HashPassword(password),
                Role = role ?? "User",
                IsActive = true
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> AssignPlaylists(int userId)
        {
            var user = await _context.Users.Include(u => u.PlaylistAccesses).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            var allPlaylists = await _context.Playlists.ToListAsync();
            var assignedIds = user.PlaylistAccesses.Select(pa => pa.PlaylistId).ToList();

            ViewBag.User = user;
            ViewBag.AssignedIds = assignedIds;
            return View(allPlaylists);
        }

        [HttpPost]
        public async Task<IActionResult> AssignPlaylists(int userId, int[] playlistIds)
        {
            var user = await _context.Users.Include(u => u.PlaylistAccesses).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return NotFound();

            // Clear existing
            _context.PlaylistAccesses.RemoveRange(user.PlaylistAccesses);

            // Add new
            foreach (var pid in playlistIds)
            {
                _context.PlaylistAccesses.Add(new PlaylistAccess { UserId = userId, PlaylistId = pid });
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Users");
        }

        // --- Playlist Management ---
        public async Task<IActionResult> Playlists()
        {
            return View(await _context.Playlists.Include(p => p.Videos).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlaylist(string name, string? description)
        {
            _context.Playlists.Add(new Playlist { Name = name, Description = description });
            await _context.SaveChangesAsync();
            return RedirectToAction("Playlists");
        }

        // --- Video Management ---
        public async Task<IActionResult> CreateVideo()
        {
            ViewBag.Playlists = new SelectList(await _context.Playlists.ToListAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateVideo(string url, string title, int playlistId)
        {
            var videoId = YouTubeHelper.ExtractVideoId(url);
            if (videoId == null)
            {
                ModelState.AddModelError("url", "Invalid YouTube URL");
                ViewBag.Playlists = new SelectList(await _context.Playlists.ToListAsync(), "Id", "Name");
                return View();
            }

            var video = new Video
            {
                Title = title,
                YouTubeVideoId = videoId,
                PlaylistId = playlistId,
                IsActive = true
            };

            _context.Videos.Add(video);
            await _context.SaveChangesAsync();
            return RedirectToAction("Playlists");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Users));
        }

        [HttpPost]
        public async Task<IActionResult> DeletePlaylist(int id)
        {
            var playlist = await _context.Playlists.FindAsync(id);
            if (playlist != null)
            {
                _context.Playlists.Remove(playlist);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Playlists));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            var video = await _context.Videos.FindAsync(id);
            if (video != null)
            {
                _context.Videos.Remove(video);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Playlists));
        }
    }
}
