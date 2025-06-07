using System.Diagnostics;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace Common.UI.WebUI.LLM
{
    public partial class ClaudeControl : IFocasable
    {
        public event EventHandler<string> ResponseGenerated;

        private string url;
        private string title = string.Empty;

        public ClaudeControl(string url)
        {
            InitializeComponent();
            this.url = url;
            ResponseGenerated += ChatGptControl_ResponseGenerated;
        }

        private static void ChatGptControl_ResponseGenerated(object? sender, string e)
        {
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
                    webView.CoreWebView2.NavigationCompleted += webView_NavigationCompleted;
                    webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

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
                this.url = value;
                webView.CoreWebView2.Navigate(url);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeWebView();
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

        private async void webView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // ナビゲーションが成功したか確認
            if (e.IsSuccess)
            {
                // 現在のページのタイトルを取得するためにJavaScriptを実行
                await webView.CoreWebView2.ExecuteScriptAsync("document.title").ContinueWith(task =>
                {
                    // JavaScriptの結果を取得
                    title = task.Result;

                    // JSON形式で返されるため、トリムしてダブルクォーテーションを削除
                    title = title.Trim('"');
                });
            }
        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var messageJson = e.WebMessageAsJson;
            var message = System.Text.Json.JsonSerializer.Deserialize<WebViewMessage>(messageJson);

            if (message?.Type == "responseGenerated")
            {
                ResponseGenerated(this, message.Content);
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
    }
}