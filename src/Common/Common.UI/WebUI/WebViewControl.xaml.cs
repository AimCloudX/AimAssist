using System.Diagnostics;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace Common.UI.WebUI
{
    public partial class WebViewControl : System.Windows.Controls.UserControl, IFocasable
    {
        private string url;
        private string readedUrl;
        private string title;

        public WebViewControl(string url, string title)
        {
            InitializeComponent();
            this.title = title;
            this.url = url;
        }

        public WebViewControl(string url)
        {
            InitializeComponent();
            this.url = url;
        }
        
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
                await webView.EnsureCoreWebView2Async(null);
                webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                webView.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = false;
                webView.CoreWebView2.Settings.IsStatusBarEnabled = false;
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

            var htmlLink = $"<a href=\"{url}\">{title}</a>";
            var titleUrl = $"[{title}]({url})";

            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, htmlLink);
            dataObject.SetData(DataFormats.Text, titleUrl);
            Clipboard.SetDataObject(dataObject);

            string bookmarklet = "javascript:(function(){alert('リンクをコピーしました');})();";
            webView.CoreWebView2.ExecuteScriptAsync(bookmarklet);
        }

        private void webView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                readedUrl = webView.Source.ToString();

                webView.CoreWebView2.ExecuteScriptAsync("document.title").ContinueWith(task =>
                {
                    title = task.Result;
                    title = title.Trim('"');
                });
            }
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            webView.CoreWebView2.Navigate(this.url);
        }
    }
}
