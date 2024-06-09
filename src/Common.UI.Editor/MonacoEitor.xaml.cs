using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System.IO;

namespace Common.UI.Editor
{
    /// <summary>
    /// MonacoEitor.xaml の相互作用ロジック
    /// </summary>
    public partial class MonacoEditor : System.Windows.Controls.UserControl
    {
        private EditorOption option = new EditorOption();
        private string text = string.Empty;

        public MonacoEditor()
        {
            this.InitializeComponent();
            this.webView.NavigationCompleted += InitializeCoreWebView2Completed;
            InitializeAsync();
        }

        private void InitializeCoreWebView2Completed(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            SetOptionInner();
            if (!string.IsNullOrEmpty(text))
            {
                string script = $"setEditorContent({JsonConvert.SerializeObject(text)});";
                webView.CoreWebView2.ExecuteScriptAsync(script);
            }
        }

        public async void SetText(string text)
        {
            this.text = text;
            if(webView.CoreWebView2 != null)
            {
                string script = $"setEditorContent({JsonConvert.SerializeObject(text)});";
                await webView.CoreWebView2.ExecuteScriptAsync(script);
            }

        }

        public async void SetOption(EditorOption option)
        {
            this.option = option;
            if(webView.CoreWebView2 != null)
            {
                SetOptionInner();
            }
        }

        private async void SetOptionInner()
        {
            switch (option.Mode)
            {
                case EditorMode.Standard:
                    await webView.CoreWebView2.ExecuteScriptAsync("toggleVimMode(false);");
                    break;
                case EditorMode.Vim:
                    await webView.CoreWebView2.ExecuteScriptAsync("toggleVimMode(true);");

                    if (!string.IsNullOrEmpty(option.CustomVimKeybindingPath))
                    {
                        var text = File.ReadAllText(option.CustomVimKeybindingPath);
                        await webView.CoreWebView2.ExecuteScriptAsync(text);
                    }

                    break;
            }

        }

        public async Task<string> GetText()
        {
            var fileContent = await webView.CoreWebView2.ExecuteScriptAsync("getEditorContent();");

            fileContent = fileContent.Trim('"').Replace("\\n", "\n").Replace("\\r", "\r"); // JSON文字列から実際の内容を取得
            return JsonConvert.DeserializeObject<string>($"\"{fileContent}\"");
        }

        private async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            string htmlFilePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Monaco", "src", "index.html");
            webView.Source = new Uri($"file:///{htmlFilePath}");
            //webView.CoreWebView2.Navigate(htmlFilePath);
        }
    }
}
