using System.Text.RegularExpressions;

namespace PrivateTube.Services
{
    public static class YouTubeHelper
    {
        public static string? ExtractVideoId(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return null;

            // Handle standard URL (https://www.youtube.com/watch?v=VIDEO_ID)
            var match = Regex.Match(url, @"(?:v=|\/)([0-9A-Za-z_-]{11}).*", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            // Handle shortened URL (https://youtu.be/VIDEO_ID)
            match = Regex.Match(url, @"youtu\.be\/([0-9A-Za-z_-]{11})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
            
            // Handle embed URL
            match = Regex.Match(url, @"embed\/([0-9A-Za-z_-]{11})", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            return null; // or throw exception
        }
    }
}
