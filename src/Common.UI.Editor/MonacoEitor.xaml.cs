using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
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
        private async void LoadFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                string fileContent = await File.ReadAllTextAsync(filePath);
                string script = $"setEditorContent({JsonConvert.SerializeObject(fileContent)});";
                webView.CoreWebView2.ExecuteScriptAsync(script);
            }
        }

        private async void SaveFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            if (saveFileDialog.ShowDialog() == true)
            {
                string filePath = saveFileDialog.FileName;
                string fileContent = await webView.CoreWebView2.ExecuteScriptAsync("getEditorContent();");
                fileContent = fileContent.Trim('"').Replace("\\n", "\n").Replace("\\r", "\r"); // JSON文字列から実際の内容を取得
                string decodedContent = JsonConvert.DeserializeObject<string>($"\"{fileContent}\"");
                await File.WriteAllTextAsync(filePath, decodedContent);
            }
        }

        private async void LoadVimrc_Click(object sender, RoutedEventArgs e)
        {
            var vimrcFile  = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Monaco","src","keybindings.js");
            var text = File.ReadAllText(vimrcFile);
            await webView.CoreWebView2.ExecuteScriptAsync(text);
        }
    }

}
