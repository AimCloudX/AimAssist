using AimAssist.Core.Commands;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.Web.BookSearch.GoogleApis;
using Common.UI;
using Common.UI.WebUI;
using Library.BookSearch;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace AimAssist.Units.Implementation.Web.BookSearch
{
    /// <summary>
    /// BookSearchControl.xaml の相互作用ロジック
    /// </summary>
    public partial class BookSearchControl : UserControl
    {
        public BookSearchControl()
        {
            InitializeComponent();
            Initialize();
            InitializeSearchParams();
        }
        private ObservableCollection<SearchParameter> _searchParams;

        private void InitializeSearchParams()
        {
            var paramter = BookSearchService.SearchParameters;
            if(paramter != null )
            {
                _searchParams = new ObservableCollection<SearchParameter>(paramter);
            }
            else
            {
                _searchParams = new ObservableCollection<SearchParameter>();
                for (int i = 0; i < 10; i++)
                {
                    _searchParams.Add(new SearchParameter());
                }
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

            foreach (var inputText in _searchParams.Where(x=>!string.IsNullOrEmpty(x.SearchText)).Select(y=>y.GetInputText()))
            {
                var units  = GetUnits(inputText);
                await foreach(var unit in units)
                {
                    yield return unit;
                }
            }

            iswebloading = false;
        }

        private async IAsyncEnumerable<IUnit> GetUnits(string inputText)
        {
            string apiUrl = $"https://www.googleapis.com/books/v1/volumes?q={inputText}";
            this.WebView.CoreWebView2.Navigate("about:blank"); // Navigate to a blank page to execute JavaScript
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

            var helper = new WebViewHelper(this.WebView.CoreWebView2);
            await this.WebView.CoreWebView2.ExecuteScriptAsync(script);


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
                            yield return new Unit(BookSearchMode.Instance, titlte, new UrlPath(url));
                        }
                    }
                }
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            BookSearchService.SearchParameters = _searchParams.ToList();
            var units = GetUnits();

            var unitLists = new List<IUnit>();
            await foreach(var unit in units)
            {
                unitLists.Add(unit);
            }

            var args = new UnitsArgs(BookSearchMode.Instance, unitLists, true);
            AimAssistCommands.SendUnitCommand.Execute(args, this);
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            BookSearchService.SearchParameters = _searchParams.ToList();
        }
    }

    public class UnitsArgs
    {
        public UnitsArgs(IMode mode, IEnumerable<IUnit> units, bool needSetMode)
        {
            Mode = mode;
            Units = units;
            NeedSetMode = needSetMode;
        }

        public IMode Mode { get; }
        public IEnumerable<IUnit> Units { get; }
        public bool NeedSetMode { get; }
    }
}
