using Microsoft.EntityFrameworkCore;
using MoodPlaylistGenerator.Data;
using MoodPlaylistGenerator.Models;
using System.Web;

namespace MoodPlaylistGenerator.Services
{
    public class SongService
    {
        private readonly ApplicationDbContext _context;

        public SongService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Song>> GetUserSongsAsync(int userId)
        {
            return await _context.Songs
                .Include(s => s.SongMoods)
                .ThenInclude(sm => sm.Mood)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Song?> GetSongByIdAsync(int songId, int userId)
        {
            return await _context.Songs
                .Include(s => s.SongMoods)
                .ThenInclude(sm => sm.Mood)
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);
        }

        public async Task<Song> CreateSongAsync(string title, string artist, string youtubeUrl, int userId, List<int> moodIds)
        {
            var song = new Song
            {
                Title = title,
                Artist = artist,
                YouTubeUrl = youtubeUrl,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Songs.Add(song);
            await _context.SaveChangesAsync();

            // Add mood associations
            if (moodIds.Any())
            {
                foreach (var moodId in moodIds)
                {
                    _context.SongMoods.Add(new SongMood
                    {
                        SongId = song.Id,
                        MoodId = moodId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return await GetSongByIdAsync(song.Id, userId) ?? song;
        }

        public async Task<Song?> UpdateSongAsync(int songId, int userId, string title, string artist, string youtubeUrl, List<int> moodIds)
        {
            var song = await _context.Songs
                .Include(s => s.SongMoods)
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);

            if (song == null)
                return null;

            // Update song properties
            song.Title = title;
            song.Artist = artist;
            song.YouTubeUrl = youtubeUrl;

            // Remove existing mood associations
            _context.SongMoods.RemoveRange(song.SongMoods);

            // Add new mood associations
            foreach (var moodId in moodIds)
            {
                song.SongMoods.Add(new SongMood
                {
                    SongId = song.Id,
                    MoodId = moodId
                });
            }

            await _context.SaveChangesAsync();
            return await GetSongByIdAsync(songId, userId);
        }

        public async Task<bool> DeleteSongAsync(int songId, int userId)
        {
            var song = await _context.Songs
                .FirstOrDefaultAsync(s => s.Id == songId && s.UserId == userId);

            if (song == null)
                return false;

            _context.Songs.Remove(song);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<Mood>> GetAllMoodsAsync()
        {
            return await _context.Moods.OrderBy(m => m.Name).ToListAsync();
        }

        public async Task<List<Song>> GetSongsByMoodAsync(int moodId, int userId)
        {
            return await _context.Songs
                .Include(s => s.SongMoods)
                .ThenInclude(sm => sm.Mood)
                .Where(s => s.UserId == userId && s.SongMoods.Any(sm => sm.MoodId == moodId))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public string ExtractYouTubeVideoId(string url)
        {
            // Extract video ID from various YouTube URL formats
            var uri = new Uri(url);
            
            if (uri.Host.Contains("youtu.be"))
            {
                return uri.AbsolutePath.TrimStart('/');
            }
            
            if (uri.Host.Contains("youtube.com"))
            {
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                return query["v"] ?? "";
            }

            return "";
        }
        
        // TODO: Implement method to create song with media file
        /*
        public async Task<Song> CreateSongWithMediaAsync(string title, string artist, int userId, List<int> moodIds, IFormFile? mediaFile = null, string? youtubeUrl = null)
        {
            // TODO: Create new Song object
            // TODO: Handle media file upload if provided
            // TODO: Set YouTube URL if no media file
            // TODO: Save to database
            // TODO: Add mood associations
        }
        */
        
        // TODO: Implement method to get playable URL
        /*
        public string GetPlayableUrl(Song song, IMediaUploadService mediaUploadService)
        {
            // TODO: Check if local media file exists
            // TODO: Return local file URL if available
            // TODO: Fallback to YouTube URL
            // TODO: Fallback to Rick Roll if nothing available
        }
        */
    }
}
