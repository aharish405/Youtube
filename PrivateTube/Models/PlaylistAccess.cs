namespace PrivateTube.Models
{
    public class PlaylistAccess
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int PlaylistId { get; set; }
        public Playlist Playlist { get; set; } = null!;
    }
}
