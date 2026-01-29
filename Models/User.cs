using System.ComponentModel.DataAnnotations;

namespace PrivateTube.Models
{
    public class User
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "User"; // "Admin" or "User"

        public bool IsActive { get; set; } = true;

        public ICollection<PlaylistAccess> PlaylistAccesses { get; set; } = new List<PlaylistAccess>();
    }
}
