using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PrivateTube.Data;
using PrivateTube.Models;
using PrivateTube.Services;
using PrivateTube.ViewModels;
using System.Security.Claims;

namespace PrivateTube.Controllers
{
    [Authorize]
    public class StudioController : Controller
    {
        private readonly AppDbContext _context;

        public StudioController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            var playlists = await _context.Playlists
                .Where(p => p.PlaylistAccesses.Any(pa => pa.UserId == userId))
                .Include(p => p.Videos)
                .ToListAsync();

            var videos = await _context.Videos
                .Include(v => v.Playlist)
                .ThenInclude(p => p.PlaylistAccesses)
                .Where(v => v.Playlist.PlaylistAccesses.Any(pa => pa.UserId == userId))
                .OrderByDescending(v => v.CreatedDate)
                .ToListAsync();

            var model = new HomeViewModel // Reuse ViewModel or create StudioViewModel
            {
                Playlists = playlists,
                Videos = videos
            };

            return View(model);
        }

        public IActionResult CreatePlaylist()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreatePlaylist(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                ModelState.AddModelError("", "Name is required");
                return View();
            }

            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            var playlist = new Playlist
            {
                Name = name,
                Description = description,
                CreatorId = userId
            };

            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();

            _context.PlaylistAccesses.Add(new PlaylistAccess
            {
                UserId = userId,
                PlaylistId = playlist.Id
            });
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditPlaylist(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var playlist = await _context.Playlists.FindAsync(id);

            if (playlist == null) return NotFound();

            bool isCreator = playlist.CreatorId == userId;
            bool isLegacyOwner = playlist.CreatorId == null && await _context.PlaylistAccesses.AnyAsync(pa => pa.UserId == userId && pa.PlaylistId == id);

            if (!isCreator && !isLegacyOwner) return Forbid();

            return View(playlist);
        }

        [HttpPost]
        public async Task<IActionResult> EditPlaylist(int id, string name, string description)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var playlist = await _context.Playlists.FindAsync(id);

            if (playlist == null) return NotFound();

            bool isCreator = playlist.CreatorId == userId;
            bool isLegacyOwner = playlist.CreatorId == null && await _context.PlaylistAccesses.AnyAsync(pa => pa.UserId == userId && pa.PlaylistId == id);

            if (!isCreator && !isLegacyOwner) return Forbid();

            playlist.Name = name;
            playlist.Description = description;

            if (playlist.CreatorId == null) playlist.CreatorId = userId;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeletePlaylist(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var playlist = await _context.Playlists.FindAsync(id);

            if (playlist == null) return NotFound();

            bool isCreator = playlist.CreatorId == userId;
            bool isLegacyOwner = playlist.CreatorId == null && await _context.PlaylistAccesses.AnyAsync(pa => pa.UserId == userId && pa.PlaylistId == id);

            if (!isCreator && !isLegacyOwner) return Forbid();

            _context.Playlists.Remove(playlist);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CreateVideo()
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            var userPlaylists = await _context.Playlists
                .Where(p => p.PlaylistAccesses.Any(pa => pa.UserId == userId))
                .Select(p => new { p.Id, p.Name })
                .ToListAsync();

            ViewBag.Playlists = new SelectList(userPlaylists, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateVideo(string url, string title, int playlistId)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            // Verify access to playlist
            var hasAccess = await _context.PlaylistAccesses
                .AnyAsync(pa => pa.UserId == userId && pa.PlaylistId == playlistId);

            if (!hasAccess) return Forbid();

            var videoId = YouTubeHelper.ExtractVideoId(url);
            if (string.IsNullOrEmpty(videoId))
            {
                ModelState.AddModelError("", "Invalid YouTube URL");
                return await CreateVideo(); // Reload view
            }

            var video = new Video
            {
                Title = title,
                YouTubeVideoId = videoId,
                PlaylistId = playlistId,
                IsActive = true,
                CreatedDate = DateTime.Now,
                CreatorId = userId
            };

            _context.Videos.Add(video);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteVideo(int id)
        {
            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var video = await _context.Videos.FindAsync(id);

            if (video == null) return NotFound();

            // Ownership check: Creator of Video OR Creator of Playlist
            var playlist = await _context.Playlists.FindAsync(video.PlaylistId);
            bool isPlaylistOwner = playlist != null && playlist.CreatorId == userId;
            bool isVideoCreator = video.CreatorId == userId;

            if (!isPlaylistOwner && !isVideoCreator) return Forbid();

            _context.Videos.Remove(video);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
