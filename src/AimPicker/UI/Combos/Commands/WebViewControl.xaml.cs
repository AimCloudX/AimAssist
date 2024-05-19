using System;
using System.Collections.Generic;
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
