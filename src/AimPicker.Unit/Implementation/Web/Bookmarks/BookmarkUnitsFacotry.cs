using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Web.Urls;
using Newtonsoft.Json;
using System.IO;

namespace AimPicker.Unit.Implementation.Web.Bookmarks
{
    public class BookmarkUnitsFacotry : IUnitsFacotry
    {
        public IPickerMode TargetMode => BookmarkMode.Instance;

        public bool IsShowInStnadard => false;

        public IEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
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
