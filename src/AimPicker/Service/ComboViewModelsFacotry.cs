using AimPicker.Combos.Mode;
using AimPicker.Combos.Mode.BookSearch.GoogleApis;
using AimPicker.Combos.Mode.Snippet;
using AimPicker.Combos.Mode.Wiki;
using AimPicker.Combos.Mode.WorkFlows;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Bookmarks;
using AimPicker.Unit.Implementation.BookSearch;
using AimPicker.Unit.Implementation.Snippets;
using AimPicker.Unit.Implementation.Urls;
using AimPicker.Unit.Implementation.Wiki;
using AimPicker.Unit.Implementation.WorkFlows;
using Common.UI;
using Common.UI.Amazon;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace AimPicker.Combos
{
    public class ComboViewModelsFacotry : IDisposable
    {
        private readonly Window window;
        private readonly WebView2 webView;
        public ComboViewModelsFacotry()
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


        public async IAsyncEnumerable<IUnitViewModel> Create(IPickerMode mode, string inputText)
        {
            switch (mode)
            {
                case NormalMode:
                    foreach (var modeCombo in ComboService.ModeComboLists)
                    {
                        yield return new ModeComboViewModel(modeCombo);

                    }
                    // 必要があれば各モードのcomboを追加する

                    break;
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
                case BookmarkMode:
                    await foreach (var combo in CreateBookMarcCombo())
                    {
                        yield return combo;
                    }
                    break;
                case KnowledgeMode:
                    var aa = new DirectoryInfo("Resources/Wiki/");
                    foreach (var file in aa.GetFiles())
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file.Name);
                        yield return new KnowledgeViewModel(fileName, file.FullName);
                    }
                    //yield return new WikiViewModel(file.Name, "C:\\Projects\\AimPicker\\README.md");

                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private async IAsyncEnumerable<IUnitViewModel> CreateBookMarcCombo()
        {
            var allBookmarks = new List<BookmarkItem>();
            var chromeBookmarksPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                               "Google\\Chrome\\User Data\\Default\\Bookmarks");

            if (File.Exists(chromeBookmarksPath))
            {
                string json = File.ReadAllText(chromeBookmarksPath);
                var bookmarkData = JsonConvert.DeserializeObject<ChromeBookmarks>(json);

                var bookmarks = BookmarkItemConverter.Convert(bookmarkData.Roots.BookmarkBar.Children);
                allBookmarks.AddRange(bookmarks);
                var others = BookmarkItemConverter.Convert(bookmarkData.Roots.Other.Children);
                allBookmarks.AddRange(others);
            }
            var vivlaldiBookmarksPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                               "Vivaldi\\User Data\\Default\\Bookmarks");

            if (File.Exists(vivlaldiBookmarksPath))
            {
                string json = File.ReadAllText(vivlaldiBookmarksPath);
                var bookmarkData = JsonConvert.DeserializeObject<ChromeBookmarks>(json);

                var bookmarks = BookmarkItemConverter.Convert(bookmarkData.Roots.BookmarkBar.Children);
                allBookmarks.AddRange(bookmarks);
                var others = BookmarkItemConverter.Convert(bookmarkData.Roots.Other.Children);
                allBookmarks.AddRange(others);
            }

            foreach (var bookmark in allBookmarks)
            {
                if (!bookmark.URL.StartsWith("http"))
                {
                    continue;
                }
                yield return new UrlCommandViewModel(bookmark.FullPath, bookmark.URL, new WebViewPreviewFactory());
            }

        }

        private async static IAsyncEnumerable<IUnitViewModel> CreateUrlCobo(string inputText)
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

        private static async IAsyncEnumerable<IUnitViewModel> CreateSnippetCombo()
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
                    yield return new SnippetViewModel(snippet.Name, snippet.Text);
                }
            }
        }

        private async IAsyncEnumerable<IUnitViewModel> CreateWorkFlowCombo()
        {
            var combos = ComboService.ComboDictionary2[WorkFlowMode.Instance];
            foreach (var combo in combos)
            {
                if (combo is WorkFlowCombo workFlow)
                {
                    yield return new PickerCommandViewModel(workFlow.Name, workFlow.Text, workFlow.PreviewFactory);
                }
            }
        }

        private async IAsyncEnumerable<IUnitViewModel> CreateBookSearchBombo(string inputText)
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
            window.Close();
        }

        private bool iswebloading;

        static void PrintBookmarks(List<ChromeBookmarkNode> bookmarks)
        {
            foreach (var bookmark in bookmarks)
            {
                if (bookmark.Type == "folder")
                {
                    Console.WriteLine("Folder: " + bookmark.Name);
                    PrintBookmarks(bookmark.Children);
                }
                else if (bookmark.Type == "url")
                {
                    Console.WriteLine("Bookmark: " + bookmark.Name + " - " + bookmark.Url);
                }
            }
        }
    }

    public class ChromeBookmarks
    {
        [JsonProperty("roots")]
        public ChromeRoots Roots { get; set; }
    }

    public class ChromeRoots
    {
        [JsonProperty("bookmark_bar")]
        public ChromeBookmarkNode BookmarkBar { get; set; }

        [JsonProperty("other")]
        public ChromeBookmarkNode Other { get; set; }

        [JsonProperty("synced")]
        public ChromeBookmarkNode Synced { get; set; }
    }

    public class ChromeBookmarkNode
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("children")]
        public List<ChromeBookmarkNode> Children { get; set; }
    }

    public class BookmarkItem
    {
        public string FullPath { get; }
        public string URL { get; }

        public BookmarkItem(string fullPath, string url)
        {
            FullPath = fullPath;
            URL = url;
        }
    }

    public class BookmarkItemConverter
    {
        public static List<BookmarkItem> Convert(List<ChromeBookmarkNode> bookmarkNodes, string parentPath = "")
        {
            var bookmarkItems = new List<BookmarkItem>();

            foreach (var node in bookmarkNodes)
            {
                var fullPath = CombinePath(parentPath, node.Name);
                if (node.Type == "url")
                {
                    bookmarkItems.Add(new BookmarkItem(fullPath, node.Url));
                }
                else if (node.Type == "folder")
                {
                    bookmarkItems.AddRange(Convert(node.Children, fullPath));
                }
            }

            return bookmarkItems;
        }

        private static string CombinePath(string parentPath, string nodeName)
        {
            if (string.IsNullOrEmpty(parentPath))
                return nodeName;
            return parentPath + "/" + nodeName;
        }
    }

}
