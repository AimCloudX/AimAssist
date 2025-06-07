using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AimAssist.Units.Implementation.Computer.Services
{
    public class IpInfoService
    {
        public async Task<IpInfo> GetIpInfoAsync()
        {
            using var client = new HttpClient();
            string response = await client.GetStringAsync("https://ipinfo.io/json");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            };
            
            return JsonSerializer.Deserialize<IpInfo>(response, options) ?? new IpInfo();
        }
    }

    public class IpInfo
    {
        public string Ip { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Loc { get; set; } = string.Empty;
        public string Org { get; set; } = string.Empty;
        public string Postal { get; set; } = string.Empty;
        public string Timezone { get; set; } = string.Empty;
        public string Readme { get; set; } = string.Empty;

        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; } = new Dictionary<string, JsonElement>();
    }
}
