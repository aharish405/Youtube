using PrivateTube.Models;

namespace PrivateTube.ViewModels
{
    public class WatchViewModel
    {
        public Video CurrentVideo { get; set; } = null!;
        public List<Video> RecommendedVideos { get; set; } = new();
        public Video? NextVideo { get; set; }
    }
}
