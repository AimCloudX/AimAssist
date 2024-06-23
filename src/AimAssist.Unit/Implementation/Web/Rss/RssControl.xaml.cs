using AimAssist.Core.Commands;
using AimAssist.Core.Rss;
using AimAssist.Unit.Core;
using AimAssist.Unit.Implementation.Web.BookSearch;
using AimAssist.Unit.Implementation.Web.Urls;
using Common.UI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace AimAssist.Unit.Implementation.Web.Rss
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
        }
        private ObservableCollection<RssSetting> _searchParams;

        private void InitializeSearchParams()
        {
            _searchParams = new ObservableCollection<RssSetting>() { 
            new RssSetting("zenn","https://zenn.dev/feed"){ IsEnabled = true},
            new RssSetting("lifehacker","https://www.lifehacker.jp/feed/index.xml"){IsEnabled = true},
            new RssSetting("ビジネスジャーナル","https://biz-journal.jp/index.xml") { IsEnabled = true },
            new RssSetting("ビジネス+IT","https://www.sbbit.jp/rss/HotTopics.rss") { IsEnabled = true },
            };
            for (int i = 4; i < 10; i++)
            {
                _searchParams.Add(new RssSetting("",""));
            }

            SearchParamsGrid.ItemsSource = _searchParams;
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


            foreach (var inputText in _searchParams.Where(x => x.IsEnabled && !string.IsNullOrEmpty(x.SearchUrl)).Select(y => y.GetCategoryUrl))
            {
                var units  = GetUnitsInner(inputText);
                await foreach(var unit in units)
                {
                    yield return unit;
                }
            }

            iswebloading = false;
        }
        static async IAsyncEnumerable<UrlUnit> GetUrlsFromRss(CategoryUrl rssUrl)
        {
            if (TryGetFeed(rssUrl, out var feed))
            {
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
