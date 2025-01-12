using AimAssist.Core.Commands;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.Web;
using AimAssist.Units.Implementation.Web.MindMeister;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Common.UI.WebUI
{
    /// <summary>
    /// MindMeisterViewControl.xaml の相互作用ロジック
    /// </summary>
    public partial class MindMeisterViewControl : System.Windows.Controls.UserControl, IFocasable
    {
        private string ApiKey;
        private string apiKeyFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mindmeisterap.dat");
        public MindMeisterViewControl(MindMeisterUnit unit)
        {
            InitializeComponent();
            this.url = unit.Path;
        }

        private string url;
        private string readedURl;
        private string title;
        private ApiService apiService;

        public async void Focus()
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
                // WebView2を初期化
                await webView.EnsureCoreWebView2Async(null);
                //ダイアログ表示を抑止
                //webView.CoreWebView2.Settings.AreDefaultScriptDialogsEnabled = false;
                //コンテキストメニューを抑止
                //webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                //開発者ツールを無効化
                webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                //ブラウザに組み込まれているエラーページを無効化
                webView.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = false;
                //ズームコントロールを無効化
                //webView.CoreWebView2.Settings.IsZoomControlEnabled = false;
                //ステータスバーを非表示
                webView.CoreWebView2.Settings.IsStatusBarEnabled = false;

                // ナビゲートするURLを設定
                webView.CoreWebView2.Navigate(url);
            }
            catch (Exception ex)
            {
            }
        }

        public string Url { get => this.url; set
            {
                this.url = value;
                webView.CoreWebView2.Navigate(url);
            } }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeWebView();
            LoadApiKey();
            if (ApiKey != null)
            {
                apiService = new ApiService(ApiKey);
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

            // Use Parallel.ForEachAsync to process map requests concurrently
            var unitLists = await Task.WhenAll(ids.Select(async id =>
            {
                return  await apiService.GetMap(id);
            }));

            var maps = unitLists.Where(x => x != null).ToArray();
            SendUnits(maps);

            apiService.AddMaps(maps);

            // test

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
                string apiUrl = "https://www.mindmeister.com/api/v2/mm.folders.getList";
                //string apiUrl = "https://www.mindmeister.com/api/v2/maps/3236607029";

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
            if (!maps.Any())
            {
                return;
            }

            var args = new UnitsArgs(MindMeisterMode.Instance, maps.Select(y => new UrlUnit(MindMeisterMode.Instance, y.Title, y.Url)).ToList(), true);
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
        return System.Text.Json.JsonSerializer.Deserialize<List<string>>(result);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error fetching map IDs: {ex.Message}");
        return new List<string>();
    }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(title))
            {

                var bookmarklet1 = "javascript:(function(){alert('リンクコピーに失敗しました');})();";
                webView.CoreWebView2.ExecuteScriptAsync(bookmarklet1);
                return;
            }

            // HTMLリンクとMarkdownリンクを生成
            var htmlLink = $"<a href=\"{url}\">{title}</a>";
            var titleUrl = $"[{title}]({url})";

            // クリップボードに書き込む
            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, htmlLink);
            dataObject.SetData(DataFormats.Text, titleUrl);
            Clipboard.SetDataObject(dataObject);

            string bookmarklet = "javascript:(function(){alert('リンクをコピーしました');})();";
            webView.CoreWebView2.ExecuteScriptAsync(bookmarklet);
        }

        private void webView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // ナビゲーションが成功したか確認
            if (e.IsSuccess)
            {
                // 現在のURLを取得
                readedURl = webView.Source.ToString();

                // 現在のページのタイトルを取得するためにJavaScriptを実行
                webView.CoreWebView2.ExecuteScriptAsync("document.title").ContinueWith(task =>
                {
                    // JavaScriptの結果を取得
                    title = task.Result;

                    // JSON形式で返されるため、トリムしてダブルクォーテーションを削除
                    title = title.Trim('"');

                });
            }
            else
            {
            }
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            // デフォルトのブラウザでURLを開く
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
                    string apiKey = Encoding.UTF8.GetString(apiKeyBytes);
                    this.ApiKey = apiKey;
                }
                catch (Exception ex)
                {
                    this.ApiKey = null;
                    StatusTextBlock.Text = $"APIキーの読み込みに失敗しました: {ex.Message}";
                }
            }
            else
            {
                this.ApiKey = null;
            }
        }
        private void SaveApiKey(string apiKey)
        {
            byte[] apiKeyBytes = Encoding.UTF8.GetBytes(apiKey);
            byte[] encryptedApiKey = ProtectedData.Protect(apiKeyBytes, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(apiKeyFilePath, encryptedApiKey);
        }

        private void MenuItem_InputApiKey_Click(object sender, RoutedEventArgs e)
        {
            var apiKeyWindow = new ApiKeyInputWindow();
            if (apiKeyWindow.ShowDialog() == true)
            {
                var apiKey = apiKeyWindow.ApiKey;
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    SaveApiKey(apiKey);
                    LoadApiKey();
                    if(ApiKey != null)
                    {
                        apiService = new ApiService(ApiKey);
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

        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            webView.CoreWebView2.Navigate(this.url);
        }
    }
}
