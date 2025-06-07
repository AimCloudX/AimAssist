using System.Diagnostics;
using System.IO;
using System.Windows;
using AimAssist.Core.Attributes;
using Common.UI;
using Microsoft.Web.WebView2.Core;

namespace AimAssist.Units.Implementation.Web.MindMeister
{
    [AutoDataTemplate(typeof(MindMeisterItemUnit), true)]
    public partial class MindMeisterViewItemControl : IFocasable
    {
        private readonly string url;
        private string? title;

        public MindMeisterViewItemControl(MindMeisterUnit unit)
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
            if (webView.CoreWebView2 != null)
            {
                webView.CoreWebView2.Navigate(this.url);
            }
        }
    }
}
