using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.Windows;

namespace AimPicker.UI.Combos.Commands
{
    /// <summary>
    /// WebViewControl.xaml の相互作用ロジック
    /// </summary>
    public partial class WebViewControl : System.Windows.Controls.UserControl
    {
        private string url;
        private string readedURl;
        private string title;

        public WebViewControl(string url)
        {
            InitializeComponent();
            this.url = url;
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

                // ナビゲートするURLを設定
                webView.CoreWebView2.Navigate(this.url);
            }
            catch (Exception ex)
            {
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeWebView();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(this.url)|| string.IsNullOrEmpty(this.title))
            {

                var bookmarklet1 = "javascript:(function(){alert('リンクコピーに失敗しました');})();";
                webView.CoreWebView2.ExecuteScriptAsync(bookmarklet1);
                return;
            }

            // HTMLリンクとMarkdownリンクを生成
            var htmlLink = $"<a href=\"{url}\">{title}</a>";
            var titleUrl = $"[{title}]({url})";

            // クリップボードに書き込む
            var dataObject = new System.Windows.DataObject();
            dataObject.SetData(System.Windows.DataFormats.Html, htmlLink);
            dataObject.SetData(System.Windows.DataFormats.Text, titleUrl);
            System.Windows.Clipboard.SetDataObject(dataObject);

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
                FileName = this.url,
                UseShellExecute = true
            });


        }
    }
}
