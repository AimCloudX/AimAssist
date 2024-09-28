using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.Windows;

namespace Common.UI
{
    /// <summary>
    /// WebViewControl.xaml の相互作用ロジック
    /// </summary>
    public partial class WebViewControl : System.Windows.Controls.UserControl, IFocasable
    {
        private string url;
        private string readedURl;
        private string title;

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
    }
}
