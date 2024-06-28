using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

namespace Common.UI.ChatGPT
{
    /// <summary>
    /// ChatGptControl.xaml の相互作用ロジック
    /// </summary>
    public partial class ChatGptControl : UserControl, IFocasable
    {

        public event EventHandler<string> ResponseGenerated;

        private string url;
        private string readedURl;
        private string title;

        public ChatGptControl(string url)
        {
            InitializeComponent();
            this.url = url;
            ResponseGenerated += ChatGptControl_ResponseGenerated;
        }

        private void ChatGptControl_ResponseGenerated(object? sender, string e)
        {
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
                // WebView2を初期化
                await webView.EnsureCoreWebView2Async(null);
                webView.CoreWebView2.NavigationCompleted += webView_NavigationCompleted;
                webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

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

        private async void webView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
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

        //    await webView.CoreWebView2.ExecuteScriptAsync(@"
        //    function observeResponseGeneration() {
        //        const targetNode = document.querySelector('main');
        //        if (!targetNode) {
        //            console.error('Target node not found');
        //            return;
        //        }

        //        const config = { childList: true, subtree: true };
        //        const callback = function(mutationsList, observer) {
        //            for(let mutation of mutationsList) {
        //                if (mutation.type === 'childList') {
        //                    const addedNodes = mutation.addedNodes;
        //                    for (let node of addedNodes) {
        //                        if (node.nodeType === Node.ELEMENT_NODE && node.classList.contains('markdown')) {
        //                            // Response generation seems to be completed
        //                            chrome.webview.postMessage({type: 'responseGenerated', content: node.innerText});
        //                            observer.disconnect(); // Stop observing once we detect the response
        //                        }
        //                    }
        //                }
        //            }
        //        };

        //        const observer = new MutationObserver(callback);
        //        observer.observe(targetNode, config);
        //    }

        //    observeResponseGeneration();
        //");
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var messageJson = e.WebMessageAsJson;
            var message = System.Text.Json.JsonSerializer.Deserialize<WebViewMessage>(messageJson);

            if (message.type == "responseGenerated")
            {
                ResponseGenerated?.Invoke(this, message.content);
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

    public class WebViewMessage
    {
        public string type { get; set; }
        public string content { get; set; }
    }

}
