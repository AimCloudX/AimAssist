using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Snippets;
using AimPicker.Unit.Implementation.Standard;
using AimPicker.Unit.Implementation.Web;
using AimPicker.Unit.Implementation.Web.Bookmarks;
using AimPicker.Unit.Implementation.Web.BookSearch;
using AimPicker.Unit.Implementation.Web.BookSearch.GoogleApis;
using AimPicker.Unit.Implementation.Web.Urls;
using AimPicker.Unit.Implementation.Wiki;
using AimPicker.Unit.Implementation.WorkFlows;
using Common.UI;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System.Windows;

namespace AimPicker.Combos
{
    public class UnitsFacotry :IDisposable
    {
        private IList<IUnitsFacotry> unitsFacotries = new List<IUnitsFacotry>();

        private readonly Window window;
        private readonly WebView2 webView;
        public UnitsFacotry()
        {
            window = new Window();
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

        public void RegisterFactory (IUnitsFacotry factory)
        {
            unitsFacotries.Add(factory);
        }

        public async IAsyncEnumerable<IUnit> Create(IPickerMode mode, string inputText)
        {
            var paramter = new UnitsFactoryParameter(inputText);
            switch (mode)
            {
                case StandardMode:
                    foreach (var factory in this.unitsFacotries.Where(x=>x.IsShowInStnadard))
                    {
                        foreach (var units in factory.GetUnits(paramter))
                        {
                            yield return units;
                        }
                    }

                    break;
                case SnippetMode:
                case WorkFlowMode:
                case UrlMode:
                case BookmarkMode:
                case KnowledgeMode:
                    foreach (var factory in this.unitsFacotries.Where(x=>x.TargetMode == mode))
                    {
                        foreach (var units in factory.GetUnits(paramter))
                        {
                            yield return units;
                        }
                    }

                    break;
                case BookSearchMode:
                    await foreach (var combo in CreateBookSearch(inputText))
                    {
                        yield return combo;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        private async IAsyncEnumerable<IUnit> CreateBookSearch(string inputText)
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
                            yield return new UrlUnit(titlte, url, new WebViewPreviewFactory());
                        }
                    }
                }
            }

            iswebloading = false;
        }

        public void Dispose()
        {
            window.Close();
        }

        private bool iswebloading;
    }
}
