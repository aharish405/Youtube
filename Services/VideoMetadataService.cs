using Microsoft.EntityFrameworkCore;
using PrivateTube.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PrivateTube.Services
{
    public class VideoMetadataService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VideoMetadataService> _logger;
        private readonly HttpClient _httpClient;

        public VideoMetadataService(IServiceProvider serviceProvider, ILogger<VideoMetadataService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessVideosAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing video metadata.");
                }

                // Wait 1 hour before next run or aggressive scan
                await Task.Delay(TimeSpan.FromMinutes(60), stoppingToken);
            }
        }

        private async Task ProcessVideosAsync(CancellationToken token)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                
                // Find videos that need updates (e.g. Placeholder titles or specific flag, here we just check for generic titles or empty)
                // For this demo, let's try to update videos that might just be urls or basic. 
                // Or just iterate all active videos to ensure accuracy.
                
                var videos = await context.Videos.ToListAsync(token);

                foreach (var video in videos)
                {
                    if (token.IsCancellationRequested) break;
                    
                    try 
                    {
                        var info = await FetchOEmbedInfo(video.YouTubeVideoId);
                        if (info != null && !string.IsNullOrWhiteSpace(info.Title))
                        {
                             // Only update if changed
                             if (video.Title != info.Title)
                             {
                                 video.Title = info.Title;
                                 context.Update(video);
                                 // We could also store thumbnail url or author if we added fields for it
                             }
                        }
                    }
                    catch (Exception ex)
                    {
                         _logger.LogWarning($"Failed to update metadata for {video.YouTubeVideoId}: {ex.Message}");
                    }
                }
                
                await context.SaveChangesAsync(token);
            }
        }

        private async Task<OEmbedResponse?> FetchOEmbedInfo(string videoId)
        {
            var url = $"https://www.youtube.com/oembed?url=https://www.youtube.com/watch?v={videoId}&format=json";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<OEmbedResponse>(json);
        }

        private class OEmbedResponse
        {
            [JsonPropertyName("title")]
            public string? Title { get; set; }
            
            [JsonPropertyName("author_name")]
            public string? AuthorName { get; set; }
            
            [JsonPropertyName("thumbnail_url")]
            public string? ThumbnailUrl { get; set; }
        }
    }
}
