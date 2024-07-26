using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Windows.Input;

namespace Common.UI.Editor
{
    /// <summary>
    /// MonacoEitor.xaml の相互作用ロジック
    /// </summary>
    public partial class MonacoEditor : System.Windows.Controls.UserControl, IFocasable
    {
        private EditorOption option = new EditorOption();
        private string text = string.Empty;
        private string language = "plaintext";
        private IEnumerable<EditorSnippet> snippets;

        public MonacoEditor()
        {
            this.InitializeComponent();
            this.webView.NavigationCompleted += InitializeCoreWebView2Completed;
            InitializeAsync();

        }

        private async void InitializeCoreWebView2Completed(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await SetOptionInner();
            if (!string.IsNullOrEmpty(text))
            {
                await SetEditorContentAsync(text, language);
            }

            if(snippets != null)
            {
                SetSnippets(this.snippets);
            }
        }

        public async Task SetTextAsync(string text, string language = "markdown")
        {
            this.text = text;
            if (language != null)
            {
                this.language = language;
            }
            if (webView.CoreWebView2 != null)
            {
                SetEditorContentAsync(text, this.language);
            }
        }
        private async Task SetEditorContentAsync(string content, string lang)
        {
            string script = $"setEditorContent({JsonConvert.SerializeObject(content)}, {JsonConvert.SerializeObject(lang)});";
            await webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        public async Task SetOptionAsync(EditorOption option)
        {
            this.option = option;
            if (webView.CoreWebView2 != null)
            {
                await SetOptionInner();
            }
        }

        private async Task SetOptionInner()
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
                        var keybindingScript = await File.ReadAllTextAsync(option.CustomVimKeybindingPath);
                        await webView.CoreWebView2.ExecuteScriptAsync(keybindingScript);
                    }
                    break;
            }
        }

        public async Task<string> GetTextAsync()
        {
            try
            {
                var fileContent = await webView.CoreWebView2.ExecuteScriptAsync("getEditorContent();");
                return JsonConvert.DeserializeObject<string>(fileContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting editor content: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task SetLanguageAsync(string language)
        {
            this.language = language;
            if (webView.CoreWebView2 != null)
            {
                string script = $"setEditorLanguage({JsonConvert.SerializeObject(language)});";
                await webView.CoreWebView2.ExecuteScriptAsync(script);
            }
        }

        public async void SetOption(EditorOption option)
        {
            this.option = option;
            if (webView.CoreWebView2 != null)
            {
                SetOptionInner();
            }
        }

        public async Task<string> GetText()
        {
            try
            {
                var fileContent = await webView.CoreWebView2.ExecuteScriptAsync("getEditorContent();");
                return JsonConvert.DeserializeObject<string>(fileContent);
            }
            catch
            {
                return string.Empty;
            }
        }

        private async void InitializeAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            string htmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Monaco", "src", "index.html");
            webView.Source = new Uri($"file:///{htmlFilePath}");
            //webView.CoreWebView2.Navigate(htmlFilePath);

            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
                window.addEventListener('keydown', (e) => {
                    window.chrome.webview.postMessage({
                        type: 'keydown',
                        key: e.key,
                        ctrlKey: e.ctrlKey,
                        shiftKey: e.shiftKey,
                        altKey: e.altKey
                    });
                });
            ");

            // WPFのキーイベントをハンドル
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;

        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P && (Keyboard.Modifiers & ModifierKeys.Control) != 0 && (Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                e.Handled = true; // イベントが処理されたことを示す
                webView.CoreWebView2.ExecuteScriptAsync("openMonacoCommandPalette();");
            }
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var jsonMessage = System.Text.Json.JsonSerializer.Deserialize<KeyEventMessage>(e.WebMessageAsJson);
            if (jsonMessage.type == "keydown" &&
                jsonMessage.key == "P" &&
                jsonMessage.ctrlKey &&
                jsonMessage.shiftKey)
            {
                // WebView2内でCtrl+Shift+Pが押されたときの処理
                webView.CoreWebView2.ExecuteScriptAsync("openMonacoCommandPalette();");
            }
        }

        public void Focus()
        {
            webView.Focus();

            // Monaco EditorのTextにフォーカスを設定するJavaScriptを実行
            var script = @"window.editor.focus();";
            webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        public void RegisterSnippets(IEnumerable<EditorSnippet> snippets)
        {
            this.snippets = snippets;
            if(webView.CoreWebView2 != null)
            {
                SetSnippets(snippets);
            }
        }

        private void SetSnippets(IEnumerable<EditorSnippet> snippets)
        {
            string snippetsJson = JsonConvert.SerializeObject(snippets);
            snippetsJson = snippetsJson.Replace("\"", "\\\""); // ダブルクォートをエスケープ
            string script = $"registerSnippets(\"{snippetsJson}\");";
            webView.CoreWebView2.ExecuteScriptAsync(script);
        }
    }

    public class EditorSnippet
    {
        public string Label { get; set; }
        public string Kind { get; set; }
        public string InsertText { get; set; }
        public string Documentation { get; set; }
    }
}


