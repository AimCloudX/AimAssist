using System.Windows;

namespace AimPicker.UI.Combos.Commands
{
    /// <summary>
    /// WebViewControl.xaml の相互作用ロジック
    /// </summary>
    public partial class WebViewControl : System.Windows.Controls.UserControl
    {
        private readonly string url;

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
    }
}
