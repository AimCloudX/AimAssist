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

        private IEnumerable<IUnit> CreateBookSearch(string inputText)
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
            var aa1 = webView.CoreWebView2.ExecuteScriptAsync(script);
            aa1.Wait();


            // 非同期にWebMessageReceivedイベントを待機
            var kk = helper.WaitForWebMessageAsync();
            kk.Wait();

            Root bookInfo = JsonConvert.DeserializeObject<Root>(kk.Result);
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

  public async void Initialize()
  {
      await webView.EnsureCoreWebView2Async(null);
  }

    public IPickerMode TargetMode => BookSearchMode.Instance;

        public bool IsShowInStnadard => false;

        public IEnumerable<IUnit> GetUnits(UnitsFactoryParameter parameter)
    {
            foreach (var aa in CreateBookSearch(parameter.InputText))
            {
                yield return aa;

            }
        }

        //private async Task<IEnumerable<IUnit>> NewMethodAsync(UnitsFactoryParameter parameter)
        //{
        //    var units = new List<IUnit>();
        //    if (iswebloading)
        //    {
        //        return units;
        //    }

        //    iswebloading = true;

        //    if (webView.CoreWebView2 == null)
        //    {
        //        iswebloading = false;
        //        return units;
        //    }

        //    if (string.IsNullOrEmpty(parameter.InputText))
        //    {
        //        iswebloading = false;
        //        return units;
        //    }

        //    string apiUrl = $"https://www.googleapis.com/books/v1/volumes?q={parameter.InputText}";
        //    webView.CoreWebView2.Navigate("about:blank"); // Navigate to a blank page to execute JavaScript

        //    string script = $@"
        //        fetch('{apiUrl}')
        //            .then(response => response.json())
        //            .then(data => {{
        //                window.chrome.webview.postMessage(data);
        //            }})
        //            .catch(error => {{
        //                console.error('Error:', error);
        //            }});
        //    ";

        //    var helper = new WebViewHelper(webView.CoreWebView2);
        //    webView.CoreWebView2.ExecuteScriptAsync(script);

        //    // 非同期にWebMessageReceivedイベントを待機
        //    var message = await helper.WaitForWebMessageAsync();

        //    var bookInfo = JsonConvert.DeserializeObject<Root>(message);
        //    if (bookInfo?.items != null)
        //    {
        //        foreach (var aa in bookInfo.items)
        //        {
        //            var title = aa.volumeInfo.title;
        //            var author = aa.volumeInfo.authors?.FirstOrDefault();
        //            if (aa.volumeInfo.industryIdentifiers == null)
        //            {
        //                continue;
        //            }
        //            foreach (var bb in aa.volumeInfo.industryIdentifiers)
        //            {
        //                if (bb.type == "ISBN_10")
        //                {
        //                    var url = $"https://www.amazon.co.jp/dp/{bb.identifier}";
        //                    units.Add(new UrlUnit(title, url, new WebViewPreviewFactory()));
        //                }
        //            }
        //        }
        //    }
        //    iswebloading = false;
        //    return units;
        //}
    }
    //public class BookSearchUnitsFactory : IUnitsFacotry
    //{
    //    public BookSearchUnitsFactory()
    //    {
    //        window = new Window();
    //        window.Height = 0;
    //        window.Width = 0;
    //        window.ShowInTaskbar = false;
    //        window.ResizeMode = ResizeMode.NoResize;
    //        window.WindowStyle = WindowStyle.None;
    //        webView = new WebView2();
    //        webView.Height = 0;
    //        webView.Width = 0;
    //        window.Content = webView;
    //        window.Show();
    //        Initialize();
    //    }

    //    public void Dispose()
    //    {
    //        window.Close();
    //    }

    //    public async void Initialize()
    //    {
    //        await webView.EnsureCoreWebView2Async(null);
    //    }

    //    public IPickerMode TargetMode => BookSearchMode.Instance;
    //    private bool iswebloading;
    //    private Window window;
    //    private WebView2 webView;

    //    public bool IsShowInStnadard => false;


    //    public IEnumerable<IUnit> GetUnits(UnitsFactoryParameter parameter)
    //    {
    //        var unitsTask = NewMethodAsync(parameter);
    //        unitsTask.Wait();

    //        // 結果を列挙して返す
    //        return unitsTask.Result;
    //    }
    //    private async Task<IEnumerable<IUnit>> NewMethodAsync(UnitsFactoryParameter parameter)
    //    {
    //        var units = new List<IUnit>();
    //        if (iswebloading)
    //        {
    //            return units;
    //        }

    //        iswebloading = true;

    //        if (webView.CoreWebView2 == null)
    //        {
    //            iswebloading = false;
    //            return units;
    //        }

    //        if (string.IsNullOrEmpty(parameter.InputText))
    //        {
    //            iswebloading = false;
    //            return units;
    //        }

    //        string apiUrl = $"https://www.googleapis.com/books/v1/volumes?q={parameter.InputText}";
    //        webView.CoreWebView2.Navigate("about:blank"); // Navigate to a blank page to execute JavaScript
    //        string script = $@"
    //            fetch('{apiUrl}')
    //                .then(response => response.json())
    //                .then(data => {{
    //                    window.chrome.webview.postMessage(data);
    //                }})
    //                .catch(error => {{
    //                    console.error('Error:', error);
    //                }});
    //        ";

    //        var helper = new WebViewHelper(webView.CoreWebView2);
    //        await webView.CoreWebView2.ExecuteScriptAsync(script);

    //        // 非同期にWebMessageReceivedイベントを待機
    //        var message = await helper.WaitForWebMessageAsync();

    //        var bookInfo = JsonConvert.DeserializeObject<Root>(message);
    //        if (bookInfo?.items != null)
    //        {
    //            foreach (var aa in bookInfo.items)
    //            {
    //                var titlte = aa.volumeInfo.title;
    //                var author = aa.volumeInfo.authors?.FirstOrDefault();
    //                if (aa.volumeInfo.industryIdentifiers == null)
    //                {
    //                    continue;
    //                }
    //                foreach (var bb in aa.volumeInfo.industryIdentifiers)
    //                {
    //                    if (bb.type == "ISBN_10")
    //                    {
    //                        var url = $"https://www.amazon.co.jp/dp/{bb.identifier}";
    //                        units.Add(new UrlUnit(titlte, url, new WebViewPreviewFactory()));
    //                    }
    //                }
    //            }
    //        }
    //        iswebloading = false;
    //        return units;
    //    }
    //}
}
