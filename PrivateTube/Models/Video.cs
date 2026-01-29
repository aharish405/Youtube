using System.ComponentModel.DataAnnotations;

namespace PrivateTube.Models
{
    public class Video
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string YouTubeVideoId { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Foreign Key
        public int PlaylistId { get; set; }
        public Playlist Playlist { get; set; } = null!;

        // Ownership
        public int? CreatorId { get; set; }
        public User? Creator { get; set; }
    }
}
