using Microsoft.Web.WebView2.Core;
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

namespace Common.UI.Editor
{
    /// <summary>
    /// MonacoEitor.xaml の相互作用ロジック
    /// </summary>
    public partial class MonacoEditor : UserControl
    {
        public MonacoEditor()
        {
         InitializeComponent();
            webView.NavigationCompleted += WebView_NavigationCompleted;
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            string htmlFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Monaco", "src", "index.html");
            webView.CoreWebView2.Navigate(htmlFilePath);
        }

        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // オプション: ナビゲーション完了イベントを処理
        }    
    private void VimModeCheckBox_Checked(object sender, RoutedEventArgs e)
    {
        webView.CoreWebView2.ExecuteScriptAsync("toggleVimMode(true);");
    }

    private void VimModeCheckBox_Unchecked(object sender, RoutedEventArgs e)
    {
        webView.CoreWebView2.ExecuteScriptAsync("toggleVimMode(false);");
    }
    }

}
