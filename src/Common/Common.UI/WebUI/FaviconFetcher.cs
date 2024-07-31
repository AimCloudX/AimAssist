using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Common.UI.WebUI
{
    public class FaviconFetcher
    {
        private static readonly HttpClient httpClient = new HttpClient();

        static FaviconFetcher()
        {
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        }

        public static BitmapImage GetUrlIconAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Invalid URL unit or description.");
            }

            try
            {
                string faviconUrl = GetFaviconUrlAsync(url);
                byte[] data = httpClient.GetByteArrayAsync(faviconUrl).Result;

                using (var ms = new MemoryStream(data))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = ms;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();
                    return bitmap;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private static string GetFaviconUrlAsync(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                throw new ArgumentException("Domain cannot be null or empty.", nameof(domain));
            }

            domain = domain.Trim().ToLowerInvariant();
            if (!Uri.TryCreate(domain, UriKind.Absolute, out var uri))
            {
                uri = new Uri($"https://{domain}");
            }

            string cleanDomain = uri.Host;

            // Try Google favicon service first
            string googleFaviconUrl = $"https://www.google.com/s2/favicons?domain={Uri.EscapeDataString(cleanDomain)}&sz=64";
            if (IsValidImageUrl(googleFaviconUrl))
            {
                return googleFaviconUrl;
            }

            // Try direct favicon URL
            string directFaviconUrl = $"{uri.Scheme}://{cleanDomain}/favicon.ico";
            if (IsValidImageUrl(directFaviconUrl))
            {
                return directFaviconUrl;
            }

            // Fallback to DuckDuckGo favicon service
            return $"https://icons.duckduckgo.com/ip3/{cleanDomain}.ico";
        }

        private static bool IsValidImageUrl(string url)
        {
            try
            {
                var response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url)).Result;
                return response.IsSuccessStatusCode && response.Content.Headers.ContentType?.MediaType.StartsWith("image/") == true;
            }
            catch
            {
                return false;
            }
        }
    }
}
