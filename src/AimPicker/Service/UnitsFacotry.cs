using AimPicker.Combos.Mode.Snippet;
using AimPicker.Combos.Mode.Wiki;
using AimPicker.Service;
using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Snippets;
using AimPicker.Unit.Implementation.Web;
using AimPicker.Unit.Implementation.Web.Bookmarks;
using AimPicker.Unit.Implementation.Web.Bookmarks.GoogleApis;
using AimPicker.Unit.Implementation.Web.BookSearch;
using AimPicker.Unit.Implementation.Web.Urls;
using AimPicker.Unit.Implementation.Wiki;
using AimPicker.Unit.Implementation.WorkFlows;
using Common.UI;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System.IO;
using System.Windows;

namespace AimPicker.Combos
{
    public class UnitsFacotry : IDisposable
    {
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


        public async IAsyncEnumerable<IUnit> Create(IPickerMode mode, string inputText)
        {
            switch (mode)
            {
                case NormalMode:
                    foreach (var modeCombo in UnitService.ModeLists.Where(x => x.IsAddUnitLists))
                    {
                        yield return new ModeChangeUnit(modeCombo);

                    }
                    // 必要があれば各モードのcomboを追加する
                    await foreach (var combo in CreateSnippetCombo())
                    {
                        yield return combo;
                    }

                    await foreach (var combo in CreateWorkFlow())
                    {
                        yield return combo;
                    }

                    var info = new DirectoryInfo("Resources/Knowledge/");
                    foreach (var file in info.GetFiles())
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file.Name);
                        yield return new KnowledgeUnit(fileName, file.FullName);
                    }

                    break;
                case SnippetMode:
                    await foreach (var combo in CreateSnippetCombo())
                    {
                        yield return combo;
                    }
                    break;
                case WorkFlowMode:
                    await foreach (var combo in CreateWorkFlow())
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
                    await foreach (var combo in CreateBookSearch(inputText))
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
                    var dictInfo = new DirectoryInfo("Resources/Knowledge/");
                    foreach (var file in dictInfo.GetFiles())
                    {
                        var fileName = Path.GetFileNameWithoutExtension(file.Name);
                        yield return new KnowledgeUnit(fileName, file.FullName);
                    }

                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private async IAsyncEnumerable<IUnit> CreateBookMarcCombo()
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
                yield return new UrlUnit(bookmark.FullPath, bookmark.URL, new WebViewPreviewFactory());
            }

        }

        private async static IAsyncEnumerable<IUnit> CreateUrlCobo(string inputText)
        {
            yield return new UrlUnit("URL Preview", inputText, new WebViewPreviewFactory());
        }

        private static async IAsyncEnumerable<IUnit> CreateSnippetCombo()
        {
            if (System.Windows.Clipboard.ContainsText())
            {
                yield return new SnippetUnit("クリップボード", System.Windows.Clipboard.GetText());
            }

            var combos = UnitService.UnitDictionary[SnippetMode.Instance];
            foreach (var combo in combos)
            {
                if (combo is SnippetUnit snippet)
                {
                    yield return new SnippetUnit(snippet.Name, snippet.Text);
                }
            }
        }

        private async IAsyncEnumerable<IUnit> CreateWorkFlow()
        {
            var combos = UnitService.UnitDictionary[WorkFlowMode.Instance];
            foreach (var combo in combos)
            {
                yield return combo;
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
