using AimPicker.Domain;
using AimPicker.DomainModels;
using AimPicker.Service;
using AimPicker.UI.Combos.Commands;
using AimPicker.UI.Combos.Snippets;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System.Windows;

namespace AimPicker.UI.Combos
{
    public class ComboViewModelsFacotry : IDisposable
    {
        private readonly Window window;
        private readonly WebView2 webView;
        public ComboViewModelsFacotry() {
            this.window = new Window();
            window.Height = 0;
            window.Width = 0;
            window.ShowInTaskbar = false;
            window.ResizeMode = ResizeMode.NoResize;
            window.WindowStyle = WindowStyle.None;
            webView = new WebView2();
            webView.Height = 0;
            webView.Width = 0;
            window.Content = webView;
            window.Show();
            Initialize();
        }

        public async void Initialize()
        {
            await webView.EnsureCoreWebView2Async(null);
        }


        public async IAsyncEnumerable<IComboViewModel> Create(IPickerMode mode, string inputText)
        {
            switch (mode)
            {
                case SnippetMode:
                    await foreach (var combo in CreateSnippetCombo())
                    {
                        yield return combo;
                    }
                    break;
                case WorkFlowMode:
                    await foreach (var combo in CreateWorkFlowCombo())
                    {
                        yield return combo;
                    }
                    break;
                case UrlMode:
                    await foreach (var combo in CreateUrlCobo(inputText))
                    {
                        yield return combo;
                    }
                    break;
                case BookSearchMode:
                    await foreach (var combo in CreateBookSearchBombo(inputText))
                    {
                        yield return combo;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private async static IAsyncEnumerable<IComboViewModel> CreateUrlCobo(string inputText)
        {
            if (inputText.StartsWith("https://www.amazon"))
            {
                yield return new UrlCommandViewModel("Amazon Preview", inputText, new AmazonWebViewPreviewFactory());
            }
            else
            {
                yield return new UrlCommandViewModel("URL Preview", inputText, new WebViewPreviewFactory());
            }
        }

        private static async IAsyncEnumerable<IComboViewModel> CreateSnippetCombo()
        {
            if (System.Windows.Clipboard.ContainsText())
            {
                yield return new SnippetViewModel("クリップボード", System.Windows.Clipboard.GetText());
            }

            var combos = ComboService.ComboDictionary2[SnippetMode.Instance];
            foreach (var combo in combos)
            {
                if (combo is SnippetCombo snippet)
                {
                    yield return new SnippetViewModel(snippet.Name, snippet.Code);
                }
            }
        }

        private async IAsyncEnumerable<IComboViewModel> CreateWorkFlowCombo()
        {
            if (System.Windows.Clipboard.ContainsText())
            {
                yield return new SnippetViewModel("クリップボード", System.Windows.Clipboard.GetText());
            }

            var combos = ComboService.ComboDictionary2[WorkFlowMode.Instance];
            foreach (var combo in combos)
            {
                if (combo is WorkFlowCombo workFlow)
                {
                    yield return new PickerCommandViewModel(workFlow.Name, workFlow.Code, workFlow.PreviewFactory);
                }
            }
        }

        private async IAsyncEnumerable<IComboViewModel> CreateBookSearchBombo(string inputText)
        {
            if (iswebloading)
            {
                yield break;
            }

            iswebloading = true;

            if (webView.CoreWebView2 == null)
            {
                iswebloading = false;
                yield break;
            }


            string apiUrl = $"https://www.googleapis.com/books/v1/volumes?q={inputText}";
            webView.CoreWebView2.Navigate("about:blank"); // Navigate to a blank page to execute JavaScript
            string script = $@"
                fetch('{apiUrl}')
                    .then(response => response.json())
                    .then(data => {{
                        window.chrome.webview.postMessage(data);
                    }})
                    .catch(error => {{
                        console.error('Error:', error);
                    }});
            ";

            var helper = new WebViewHelper(webView.CoreWebView2);
            await webView.CoreWebView2.ExecuteScriptAsync(script);


            // 非同期にWebMessageReceivedイベントを待機
            var message = await helper.WaitForWebMessageAsync();

            Root bookInfo = JsonConvert.DeserializeObject<Root>(message);
            if (bookInfo.items != null)
            {
                foreach (var aa in bookInfo.items)
                {
                    var titlte = aa.volumeInfo.title;
                    var author = aa.volumeInfo.authors?.FirstOrDefault();
                    if (aa.volumeInfo.industryIdentifiers == null)
                    {
                        continue;
                    }
                    foreach (var bb in aa.volumeInfo.industryIdentifiers)
                    {
                        if (bb.type == "ISBN_10")
                        {
                            var url = $"https://www.amazon.co.jp/dp/{bb.identifier}";
                            yield return new UrlCommandViewModel(titlte, url, new AmazonWebViewPreviewFactory());
                        }
                    }
                }
            }

            iswebloading = false;
        }

        public void Dispose()
        {
            this.window.Close();
        }

        private bool iswebloading;
    }
}
