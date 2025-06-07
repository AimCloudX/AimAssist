using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;

namespace Common.UI.WebUI
{
    public class FaviconFetcher
    {
        private static readonly HttpClient HttpClient = new();

        static FaviconFetcher()
        {
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        }

        public static async Task<BitmapImage?> GetUrlIconAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("Invalid URL unit or description.");
            }

            try
            {
                string faviconUrl = await GetFaviconUrlAsync(url);
                byte[] data = await HttpClient.GetByteArrayAsync(faviconUrl);

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
            catch (Exception)
            {
                return null;
            }
        }

        private static async Task<string> GetFaviconUrlAsync(string domain)
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

            string googleFaviconUrl =
                $"https://www.google.com/s2/favicons?domain={Uri.EscapeDataString(cleanDomain)}&sz=64";
            if (await IsValidImageUrl(googleFaviconUrl))
            {
                return googleFaviconUrl;
            }

            string directFaviconUrl = $"{uri.Scheme}://{cleanDomain}/favicon.ico";
            if (await IsValidImageUrl(directFaviconUrl))
            {
                return directFaviconUrl;
            }

            return $"https://icons.duckduckgo.com/ip3/{cleanDomain}.ico";
        }

        private static async Task<bool> IsValidImageUrl(string url)
        {
            try
            {
                var response = await HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url));
                return response.IsSuccessStatusCode &&
                       response.Content.Headers.ContentType?.MediaType?.StartsWith("image/") == true;
            }
            catch
            {
                return false;
            }
        }
    }
}