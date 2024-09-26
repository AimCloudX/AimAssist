using AimAssist.Core.Commands;
using AimAssist.Units.Core.Units;
using Library.Rss;
using System.Collections.ObjectModel;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace AimAssist.Units.Implementation.Web.Rss
{
    /// <summary>
    /// RssControl.xaml の相互作用ロジック
    /// </summary>
    public partial class RssControl : UserControl
    {
        public RssControl()
        {
            InitializeComponent();
            Initialize();
            InitializeSearchParams();
            this.DataContext = this;
        }
        public ObservableCollection<RssItemUnit> SearchParams { get; set; } = new ObservableCollection<RssItemUnit>();

        private void InitializeSearchParams()
        {
            SearchParams.Add(new RssItemUnit("zenn", "zenn", "https://zenn.dev/feed") { IsEnabled = true });
            SearchParams.Add(new RssItemUnit("CodeZine", "CodeZine", "https://codezine.jp/rss/new/20/index.xml") { IsEnabled = true });
            SearchParams.Add(new RssItemUnit("lifehacker", "lifehacker", "https://www.lifehacker.jp/feed/index.xml") { IsEnabled = true });
            SearchParams.Add(new RssItemUnit("ビジネスジャーナル", "ビジネスジャーナル", "https://biz-journal.jp/index.xml") { IsEnabled = true });
            SearchParams.Add(new RssItemUnit("ビジネス+IT", "ビジネス+IT", "https://www.sbbit.jp/rss/HotTopics.rss") { IsEnabled = true });
            SearchParams.Add(new RssItemUnit("企業テックブログRSS", "企業テックブログRSS", "https://yamadashy.github.io/tech-blog-rss-feed/feeds/rss.xml") { IsEnabled = false });
            SearchParams.Add(new RssItemUnit("現代ビジネス", "現代ビジネス", "https://gendai.media/list/feed/rss"));
            SearchParams.Add(new RssItemUnit("GIGAZINE", "GIGAZINE", "https://gigazine.net/news/rss_2.0/"));
            for (int i = 0; i < 5; i++)
            {
                SearchParams.Add(new RssItemUnit("","", ""));
            }
        }

        public async void Initialize()
        {
            await WebView.EnsureCoreWebView2Async(null);
        }

        private bool iswebloading;
        public async IAsyncEnumerable<IUnit> GetUnits()
        {
            if (iswebloading)
            {
                yield break;
            }

            iswebloading = true;

            if (this.WebView.CoreWebView2 == null)
            {
                iswebloading = false;
            }


            foreach (var inputText in SearchParams.Where(x => x.IsEnabled && !string.IsNullOrEmpty(x.SearchUrl)).Select(y => y.GetCategoryUrl))
            {
                var units  = GetUnitsInner(inputText);
                await foreach(var unit in units)
                {
                    yield return unit;
                }
            }

            iswebloading = false;
        }
        static async IAsyncEnumerable<IUnit> GetUrlsFromRss(CategoryUrl rssUrl)
        {
            if (TryGetFeed(rssUrl, out var feed))
            {
                foreach (var item in feed.Items)
                {
                    string title = item.Title.Text;
                    string url = item.Links.FirstOrDefault()?.Uri.ToString();

                    if(rssUrl.Category == "企業テックブログRSS")
                    {
                        if (item.PublishDate < DateTime.Now.AddDays(-3) || item.PublishDate > DateTime.Now)
                        {
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(url))
                    {
                        yield return new UrlUnit(RssMode.Instance, title, url, rssUrl.Category);
                    }
                }
            }
        }

        private static bool TryGetFeed(CategoryUrl rssUrl, out SyndicationFeed feed)
        {
            try
            {
                var reader = XmlReader.Create(rssUrl.Url);
                feed = SyndicationFeed.Load(reader);
                return feed != null;
            }
            catch
            {
                feed = null;
                return false;
            }
        }

        private async IAsyncEnumerable<IUnit> GetUnitsInner(CategoryUrl url)
        {
            var units = GetUrlsFromRss(url);

            await foreach (var unit in units)
            {
                yield return unit;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var units = GetUnits();

            var unitLists = new List<IUnit>();
            await foreach(var unit in units)
            {
                unitLists.Add(unit);
            }

            var args = new UnitsArgs(RssMode.Instance, unitLists, true);
            AimAssistCommands.SendUnitCommand.Execute(args, this);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
