using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Web.Urls;
using System.ServiceModel.Syndication;
using System.Xml;
using static System.Net.WebRequestMethods;

namespace AimAssist.Unit.Implementation.Web.Rss
{
    public class RssUnitsFactory : IUnitsFacotry
    {
        public IPickerMode TargetMode => RssMode.Instance;

        public bool IsShowInStnadard => false;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            // GoogleNewsは 直接urlが取得できない。(c-wizタグを仲介しているからリダイレクトぽくなってしまう)
            //string rssUrl = "https://news.google.com/rss?hl=ja&gl=JP&ceid=JP:ja";

            var urls = new CategoryUrl[] {
            //"https://rss.itmedia.co.jp/rss/2.0/itmedia_all.xml",

new("zenn","https://zenn.dev/feed"),
//"https://zenn.dev/topics/トピック名/feed",
// "https://b.hatena.ne.jp/q/トピック名?mode=rss&target=text&sort=recent"
new("lifehacker", "https://www.lifehacker.jp/feed/index.xml"),
new("biz-jornal", "https://biz-journal.jp/index.xml"),
new("sbbit", "https://www.sbbit.jp/rss/HotTopics.rss"),
            };

            foreach (var url in urls)
            {
                var units = GetUrlsFromRss(url);

                await foreach (var unit in units)
                {
                    yield return unit;
                }
            }
        }

        private class CategoryUrl
        {
            public CategoryUrl(string category, string url) {
                Category = category;
                Url = url;
            }

            public string Category { get; }
            public string Url { get; }
        }

        static async IAsyncEnumerable<UrlUnit> GetUrlsFromRss(CategoryUrl rssUrl)
        {
            using (XmlReader reader = XmlReader.Create(rssUrl.Url))
            {
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                foreach (var item in feed.Items)
                {
                    string title = item.Title.Text;
                    string url = item.Links.FirstOrDefault()?.Uri.ToString();

                    if (!string.IsNullOrEmpty(url))
                    {
                        yield return new UrlUnit(title, url, rssUrl.Category);
                    }
                }
            }

        }
    }
}
