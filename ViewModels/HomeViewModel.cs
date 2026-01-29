using PrivateTube.Models;

namespace PrivateTube.ViewModels
{
    public class HomeViewModel
    {
        public IEnumerable<Video> Videos { get; set; } = new List<Video>();
        public IEnumerable<Playlist> Playlists { get; set; } = new List<Playlist>();
    }
}
