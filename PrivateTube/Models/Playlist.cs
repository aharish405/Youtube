using System.ComponentModel.DataAnnotations;

namespace PrivateTube.Models
{
    public class Playlist
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public ICollection<Video> Videos { get; set; } = new List<Video>();
        public ICollection<PlaylistAccess> PlaylistAccesses { get; set; } = new List<PlaylistAccess>();

        // Ownership
        public int? CreatorId { get; set; }
        public User? Creator { get; set; }
    }
}
