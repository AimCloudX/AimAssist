using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using RestSharp;
using System.Text.Json;
using System.IO;
using CodeGenerator;
using System.Collections;

namespace AimAssist.Units.Implementation.Web.MindMeister
{
    public class ApiService
    {
        public string ApiKey { get; }

        public string MapCashPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mindmeisterap.cash");
        public List<MindMeisterMap> Cash = new();

        public void AddMaps(IEnumerable<MindMeisterMap> maps)
        {
            foreach (MindMeisterMap map in maps)
            {
                var old = Cash.FirstOrDefault(x => x.Url == map.Url);
                if (old != null)
                {
                    Cash.Remove(old);
                }

                Cash.Add(map);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = System.Text.Json.JsonSerializer.Serialize(Cash.OrderBy(x=>x.Title), options);
            File.WriteAllText(MapCashPath, json);
        }

        public async Task<MindMeisterMap?> GetMap(string id)
        {
            string apiUrl = $"https://www.mindmeister.com/api/v2/maps/{id}";

            var json = await GetJsonObject(ApiKey, apiUrl, id);
            if (json.TryGetValue("title", out var value))
            {
                return new MindMeisterMap(id, value.ToString());
            }

            return null;
        }

        private static async Task<JObject> GetJsonObject(string accessToken, string apiUrl, string id)
        {
            using (HttpClient client = new HttpClient())
            {
                // 認証ヘッダーを追加
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                try
                {
                    // APIリクエストを送信
                    HttpResponseMessage response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    // 応答内容を読み取る
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // JSONをパース
                    return JObject.Parse(responseBody);
                }
                catch (HttpRequestException e)
                {
                    return new JObject();
                }
            }
        }

        public ApiService(string apiKey)
        {
            ApiKey = apiKey;
        }

        public async Task<IEnumerable<MindMeisterMap>> LoadMap()
        {
            if (File.Exists(MapCashPath))
            {
                string json = File.ReadAllText(MapCashPath);
                var loadedMaps = System.Text.Json.JsonSerializer.Deserialize<List<MindMeisterMap>>(json);
                if(loadedMaps != null)
                {
                    var validatedMaps = await Task.WhenAll(loadedMaps.Select(async map =>
                    {
                        return await this.GetMap(map.Id).ConfigureAwait(false);
                    }));

                    var maps = validatedMaps.Where(x => x != null).ToArray();
                    Cash.AddRange(maps);

                    return maps;
                }
            }

            return Array.Empty<MindMeisterMap>();
        }
    }
}
