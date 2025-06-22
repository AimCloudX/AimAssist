using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Input;

namespace Common.UI.Editor
{
    /// <summary>
    /// MonacoEitor.xaml の相互作用ロジック
    /// </summary>
    public partial class MonacoEditor : IFocasable
    {
        private EditorOption option = EditorOption.Default();
        private string text = string.Empty;
        private string language = "plaintext";

        public MonacoEditor()
        {
            this.InitializeComponent();
            this.WebView.NavigationCompleted += InitializeCoreWebView2Completed;
            InitializeAsync();
        }

        private async void InitializeCoreWebView2Completed(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            await SetOptionInner();
            if (!string.IsNullOrEmpty(text))
            {
                await SetEditorContentAsync(text, language);
            }
        }

        public async Task SetTextAsync(string newtext, string newlanguage = "markdown")
        {
            this.text = newtext;
            this.language = newlanguage;
            if (WebView.CoreWebView2 != null)
            {
                await SetEditorContentAsync(newtext, this.language);
            }
        }

        private async Task SetEditorContentAsync(string content, string lang)
        {
            var script =
                $"setEditorContent({JsonConvert.SerializeObject(content)}, {JsonConvert.SerializeObject(lang)});";
            await WebView.CoreWebView2.ExecuteScriptAsync(script);
        }

        private async Task SetOptionInner()
        {
            switch (option.Mode)
            {
                case EditorMode.Standard:
                    await WebView.CoreWebView2.ExecuteScriptAsync("toggleVimMode(false);");
                    break;
                case EditorMode.Vim:
                    await WebView.CoreWebView2.ExecuteScriptAsync("toggleVimMode(true);");
                    if (!string.IsNullOrEmpty(option.CustomVimKeybindingPath))
                    {
                        var keybindingScript = await File.ReadAllTextAsync(option.CustomVimKeybindingPath);
                        await WebView.CoreWebView2.ExecuteScriptAsync(keybindingScript);
                    }

                    break;
            }
        }

        public void SetOption(EditorOption newoption)
        {
            this.option = newoption;
            if (WebView.CoreWebView2 != null)
            {
                _ = SetOptionInner();
            }
        }

        public async Task<string?> GetText()
        {
            try
            {
                var fileContent = await WebView.CoreWebView2.ExecuteScriptAsync("getEditorContent();");
                return JsonConvert.DeserializeObject<string>(fileContent);
            }
            catch
            {
                return string.Empty;
            }
        }

        private async void InitializeAsync()
        {
            await WebView.EnsureCoreWebView2Async(null);
            string htmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Monaco", "src", "index.html");
            WebView.Source = new Uri($"file:///{htmlFilePath}");
            //webView.CoreWebView2.Navigate(htmlFilePath);

            WebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            await WebView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(@"
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

        private void MainWindow_PreviewKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.P && (Keyboard.Modifiers & ModifierKeys.Control) != 0 &&
                (Keyboard.Modifiers & ModifierKeys.Shift) != 0)
            {
                e.Handled = true;
                WebView.CoreWebView2?.ExecuteScriptAsync("openMonacoCommandPalette();");
            }
        }

        private void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var jsonMessage = System.Text.Json.JsonSerializer.Deserialize<KeyEventMessage>(e.WebMessageAsJson);
            if (jsonMessage?.Type == "keydown" &&
                jsonMessage.Key == "P" &&
                jsonMessage.CtrlKey &&
                jsonMessage.ShiftKey)
            {
                WebView.CoreWebView2?.ExecuteScriptAsync("openMonacoCommandPalette();");
            }
        }

        public new void Focus()
        {
            WebView.Focus();

            var script = @"window.editor.focus();";
            WebView.CoreWebView2?.ExecuteScriptAsync(script);
        }
    }
}