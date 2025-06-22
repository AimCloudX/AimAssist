using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using AimAssist.Core.Attributes;
using AimAssist.Core.Commands;
using AimAssist.Units.Core.Units;
using Common.UI;
using Microsoft.Web.WebView2.Core;

namespace AimAssist.Units.Implementation.Web.MindMeister
{
    [AutoDataTemplate(typeof(MindMeisterUnit))]
    public partial class MindMeisterViewControl : IFocasable
    {
        private string? apiKey;
        private readonly string apiKeyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mindmeisterap.dat");
        
        private readonly string url;
        private string? title;
        private ApiService? apiService;

        public MindMeisterViewControl(MindMeisterUnit unit)
        {
            InitializeComponent();
            this.url = unit.Path;
        }

        public new async void Focus()
        {
            this.webView.Focus();
            if (webView.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync(@"
                (function() {
                    var textarea = document.getElementById('prompt-textarea');
                    if (textarea) {
                        textarea.focus();
                    } else {
                        console.error('Textarea with id prompt-textarea not found');
                    }
                })();
            ");
            }
        }

        private async void InitializeWebView()
        {
            if (this.webView.CoreWebView2 != null)
            {
                return;
            }

            try
            {
                await webView.EnsureCoreWebView2Async(null);
                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                    webView.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = false;
                    webView.CoreWebView2.Settings.IsStatusBarEnabled = false;

                    webView.CoreWebView2.Navigate(url);
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public string Url 
        { 
            get => this.url; 
            set
            {
                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.Navigate(value);
                }
            } 
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeWebView();
            LoadApiKey();
            if (apiKey != null)
            {
                apiService = new ApiService(apiKey);
                var maps = await apiService.LoadMap();
                SendUnits(maps);
            }
        }

        private async void Button_Click0(object sender, RoutedEventArgs e)
        {
            if(apiService == null)
            {
                return;
            }

            var ids = await ExtractMapIdsFromWebViewAsync(this.webView.CoreWebView2);

            var unitLists = await Task.WhenAll(ids.Select(async id =>
            {
                return await apiService.GetMap(id);
            }));

            var maps = unitLists.Where(x => x != null).Cast<MindMeisterMap>().ToArray();
            SendUnits(maps);

            apiService.AddMaps(maps);

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                string apiUrl = "https://www.mindmeister.com/api/v2/mm.folders.getList";

                try
                {
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Response:");
                        Console.WriteLine(responseBody);
                    }
                    else
                    {
                        Console.WriteLine($"Error: {response.StatusCode}");
                        string errorBody = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Details: {errorBody}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }
        }

        private void SendUnits(IEnumerable<MindMeisterMap> maps)
        {
            var mindMeisterMaps = maps as MindMeisterMap[] ?? maps.ToArray();
            if (mindMeisterMaps.Length == 0)
            {
                return;
            }

            var args = new UnitsArgs(MindMeisterMode.Instance, mindMeisterMaps.Select(y => new UrlUnit(MindMeisterMode.Instance, y.Title, y.Url)).ToList(), true);
            AimAssistCommands.SendUnitCommand.Execute(args, this);
        }

        static async Task<List<string>> ExtractMapIdsFromWebViewAsync(CoreWebView2 webView)
        {
            try
            {
                string script = @"(function() {
                    let ids = [];
                    document.querySelectorAll('[data-item-id]').forEach(el => {
                        let data = el.getAttribute('data-item-id');
                        try {
                            let parsed = JSON.parse(data);
                            if (parsed && parsed.id) {
                                ids.push(parsed.id.toString());
                            }
                        } catch (e) {
                            console.error('Error parsing data-item-id:', e);
                        }
                    });
                    return JSON.stringify(ids);
                })();";

                string result = await webView.ExecuteScriptAsync(script);
                result = result.Trim('"').Replace("\\n", "").Replace("\\\"", "\"");
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(result) ?? new List<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching map IDs: {ex.Message}");
                return new List<string>();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(title))
            {
                var bookmarklet1 = "javascript:(function(){alert('リンクコピーに失敗しました');})();";
                if (webView.CoreWebView2 != null)
                {
                    await webView.CoreWebView2.ExecuteScriptAsync(bookmarklet1);
                }
                return;
            }

            var htmlLink = $"<a href=\"{url}\">{title}</a>";
            var titleUrl = $"[{title}]({url})";

            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, htmlLink);
            dataObject.SetData(DataFormats.Text, titleUrl);
            Clipboard.SetDataObject(dataObject);

            string bookmarklet = "javascript:(function(){alert('リンクをコピーしました');})();";
            if (webView.CoreWebView2 != null)
            {
                await webView.CoreWebView2.ExecuteScriptAsync(bookmarklet);
            }
        }

        private async void webView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess && webView.CoreWebView2 != null)
            {
                try
                {
                    var titleResult = await webView.CoreWebView2.ExecuteScriptAsync("document.title");
                    title = titleResult?.Trim('"');
                }
                catch
                {
                    title = null;
                }
            }
        }

        private void Button_Click2(object? sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void LoadApiKey()
        {
            if (File.Exists(apiKeyFilePath))
            {
                try
                {
                    byte[] encryptedApiKey = File.ReadAllBytes(apiKeyFilePath);
                    byte[] apiKeyBytes = ProtectedData.Unprotect(encryptedApiKey, null, DataProtectionScope.CurrentUser);
                    string loadedApiKey = Encoding.UTF8.GetString(apiKeyBytes);
                    this.apiKey = loadedApiKey;
                }
                catch (Exception ex)
                {
                    this.apiKey = null;
                    StatusTextBlock.Text = $"APIキーの読み込みに失敗しました: {ex.Message}";
                }
            }
            else
            {
                this.apiKey = null;
            }
        }

        private void SaveApiKey(string newApiKey)
        {
            byte[] apiKeyBytes = Encoding.UTF8.GetBytes(newApiKey);
            byte[] encryptedApiKey = ProtectedData.Protect(apiKeyBytes, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(apiKeyFilePath, encryptedApiKey);
        }

        private void MenuItem_InputApiKey_Click(object? sender, RoutedEventArgs e)
        {
            var apiKeyWindow = new ApiKeyInputWindow();
            if (apiKeyWindow.ShowDialog() == true)
            {
                var inputApiKey = apiKeyWindow.ApiKey;
                if (!string.IsNullOrWhiteSpace(inputApiKey))
                {
                    SaveApiKey(inputApiKey);
                    LoadApiKey();
                    if(apiKey != null)
                    {
                        apiService = new ApiService(apiKey);
                        StatusTextBlock.Text = "APIキーを保存しました。";
                    }
                    else
                    {
                        StatusTextBlock.Text = "APIキーが空です。";
                    }
                }
                else
                {
                    StatusTextBlock.Text = "APIキーが空です。";
                }
            }
        }

        private void Button_Click3(object? sender, RoutedEventArgs e)
        {
            if (webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate(this.url);
            }
        }
    }
}
