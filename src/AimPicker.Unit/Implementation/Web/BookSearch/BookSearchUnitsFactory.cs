using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Web.BookSearch.GoogleApis;
using AimPicker.Unit.Implementation.Web.Urls;
using Common.UI;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System.Windows;

namespace AimPicker.Unit.Implementation.Web.BookSearch
{
    public class BookSearchUnitsFactory : IUnitsFacotry, IDisposable
    {
        private bool iswebloading;
        private Window window;
        private WebView2 webView;

        public BookSearchUnitsFactory()
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

        public void Dispose()
        {
            window.Close();
        }

        public async void Initialize()
        {
            await webView.EnsureCoreWebView2Async(null);
        }

        public IPickerMode TargetMode => BookSearchMode.Instance;

        public bool IsShowInStnadard => false;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter parameter)
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

            string apiUrl = $"https://www.googleapis.com/books/v1/volumes?q={parameter.InputText}";
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
            var json = await helper.WaitForWebMessageAsync();

            Root bookInfo = JsonConvert.DeserializeObject<Root>(json);
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
                            yield return new UrlUnit(titlte, url);
                        }
                    }
                }
            }

            iswebloading = false;
        }
    }
}
