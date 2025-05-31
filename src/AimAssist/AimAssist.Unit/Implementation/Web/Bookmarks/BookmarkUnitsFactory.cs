using AimAssist.Core.Units;
using Newtonsoft.Json;

namespace AimAssist.Units.Implementation.Web.Bookmarks
{
    /// <summary>
    /// ブックマークユニットのファクトリー
    /// </summary>
    public class BookmarkUnitsFactory : IUnitsFactory
    {
        /// <summary>
        /// ターゲットモードを取得します
        /// </summary>
        public IMode TargetMode => BookmarkMode.Instance;

        /// <summary>
        /// 標準表示するかどうかを取得します
        /// </summary>
        public bool IsShowInStandard => false;

        /// <summary>
        /// ユニットを取得します
        /// </summary>
        /// <returns>ユニットのコレクション</returns>
        public IEnumerable<IUnit> GetUnits()
        {
            // 実装は後で追加
            return Enumerable.Empty<IUnit>();
        }
    }

    /// <summary>
    /// Chromeブックマークデータ
    /// </summary>
    public class ChromeBookmarks
    {
        [JsonProperty("roots")]
        public ChromeRoots Roots { get; set; }
    }

    /// <summary>
    /// Chromeブックマークのルート
    /// </summary>
    public class ChromeRoots
    {
        [JsonProperty("bookmark_bar")]
        public ChromeBookmarkNode BookmarkBar { get; set; }

        [JsonProperty("other")]
        public ChromeBookmarkNode Other { get; set; }

        [JsonProperty("synced")]
        public ChromeBookmarkNode Synced { get; set; }
    }

    /// <summary>
    /// Chromeブックマークノード
    /// </summary>
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

    /// <summary>
    /// ブックマーク項目
    /// </summary>
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

    /// <summary>
    /// ブックマーク項目コンバーター
    /// </summary>
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
