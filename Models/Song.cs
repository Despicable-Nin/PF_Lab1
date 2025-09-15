using System.ComponentModel.DataAnnotations;

namespace MoodPlaylistGenerator.Models
{
    public class Song
    {
        public int Id { get; set; }
        
        [Required]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Artist { get; set; } = string.Empty;
        
        // TODO: Make YouTubeUrl optional since we now support local files
        [Url]
        public string? YouTubeUrl { get; set; }
        
        // TODO: Add properties for local media files
        public string? LocalFilePath { get; set; }
        public string? FileName { get; set; }
        // TODO: Add more properties like ContentType, FileSizeBytes, MediaType
        
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public User User { get; set; } = null!;
        public List<SongMood> SongMoods { get; set; } = new();
        public List<PlaylistSong> PlaylistSongs { get; set; } = new();
    }
}
