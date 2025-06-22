using System.Diagnostics;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace Common.UI.WebUI.LLM
{
    /// <summary>
    /// ChatGptControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ChatGptControl : IFocasable
    {
        public event EventHandler<string> ResponseGenerated;

        private string url = string.Empty;
        private string title = string.Empty;

        public ChatGptControl(string url)
        {
            InitializeComponent();
            this.url = url;
            ResponseGenerated += ChatGptControl_ResponseGenerated;
        }

        private void ChatGptControl_ResponseGenerated(object? sender, string e)
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
                // WebView2を初期化
                await webView.EnsureCoreWebView2Async(null);
                if (webView.CoreWebView2 != null)
                {
                    webView.CoreWebView2.NavigationCompleted += webView_NavigationCompleted;
                    webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                    // ナビゲートするURLを設定
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
                webView.CoreWebView2?.Navigate(url);
            }
        }

        private void UserControl_Loaded(object? sender, RoutedEventArgs e)
        {
            InitializeWebView();
        }

        private void Button_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(title))
            {
                var bookmarklet1 = "javascript:(function(){alert('リンクコピーに失敗しました');})();";
                webView.CoreWebView2?.ExecuteScriptAsync(bookmarklet1);
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
            webView.CoreWebView2?.ExecuteScriptAsync(bookmarklet);
        }

        private void webView_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                webView.CoreWebView2?.ExecuteScriptAsync("document.title").ContinueWith(task =>
                {
                    if (task.Result != null)
                    {
                        title = task.Result;
                        title = title.Trim('"');
                    }
                });
            }
        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var messageJson = e.WebMessageAsJson;
            var message = System.Text.Json.JsonSerializer.Deserialize<WebViewMessage>(messageJson);

            if (message is {Type: "responseGenerated"})
            {
                ResponseGenerated?.Invoke(this, message.Content);
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
    }

    public class WebViewMessage(string type, string content)
    {
        public string Type { get; init; } = type;
        public string Content { get; init; } = content;
    }
}