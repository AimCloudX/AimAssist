using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;

namespace Common.UI.WebUI.Amazon
{
    public partial class AmazonWebViewControl : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private string url;
        private string title;

        private string productTitle;
        private string asin;
        private BookInfo book;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool CanExectuteCommand { get; set; }

        public string Url { get => this.url;
                set { 
                this.url = value;
                webView.CoreWebView2.Navigate(url);
            } }

        public AmazonWebViewControl(string url)
        {
            InitializeComponent();
            this.url = url;
            this.DataContext = this;
        }
        private async void InitializeWebView()
        {
            if (this.webView.CoreWebView2 != null)
            {
                return;
            }

            try
            {
                // WebView2を初期化
                await webView.EnsureCoreWebView2Async(null);

                // ナビゲートするURLを設定
                webView.CoreWebView2.Navigate(this.url);
            }
            catch (Exception ex)
            {
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeWebView();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(this.url)|| string.IsNullOrEmpty(this.title))
            {

                var bookmarklet1 = "javascript:(function(){alert('リンクコピーに失敗しました');})();";
                webView.CoreWebView2.ExecuteScriptAsync(bookmarklet1);
                return;
            }

            // HTMLリンクとMarkdownリンクを生成
            var htmlLink = $"<a href=\"{url}\">{title}</a>";
            var titleUrl = $"[{title}]({url})";

            // クリップボードに書き込む
            var dataObject = new DataObject();
            dataObject.SetData(DataFormats.Html, htmlLink);
            dataObject.SetData(DataFormats.Text, titleUrl);
            Clipboard.SetDataObject(dataObject);

            string bookmarklet = "javascript:(function(){alert('リンクをコピーしました');})();";
            webView.CoreWebView2.ExecuteScriptAsync(bookmarklet);
        }

        private async void webView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // ナビゲーションが成功したか確認
            if (e.IsSuccess)
            {
                // 現在のページのタイトルを取得するためにJavaScriptを実行
                webView.CoreWebView2.ExecuteScriptAsync("document.title").ContinueWith(task =>
                {
                    // JavaScriptの結果を取得
                    title = task.Result;

                    // JSON形式で返されるため、トリムしてダブルクォーテーションを削除
                    title = title.Trim('"');

                });


                // productTitle
            var productTitelScript = @"
                var productTitleElement = document.getElementById('productTitle');
                var productTitle = productTitleElement ? productTitleElement.innerText.trim() : '';
                productTitle;
            ";
                webView.CoreWebView2.ExecuteScriptAsync(productTitelScript).ContinueWith(task =>
                {
                    productTitle = task.Result;
                    productTitle = productTitle.Trim('"');
                });

                string asinScript = @"
var ASINElement = document.querySelector('#ASIN');
var ASIN = ASINElement ? ASINElement.value : '';
ASIN;
";

                webView.CoreWebView2.ExecuteScriptAsync(asinScript).ContinueWith(task =>
                {
                    asin = task.Result;
                    asin = asin.Trim('"');
                });

                book = await FetchBookInfoAsync();

                this.CanExectuteCommand = true;
                OnPropertyChanged(nameof(CanExectuteCommand));

            }
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            if(book == null)
            {
                string bookmarklet = "javascript:(function(){alert('書籍情報の取得に失敗しました');})();";
                webView.CoreWebView2.ExecuteScriptAsync(bookmarklet);
                return;
            }

            var price = ConvertStringToInt(book.price);

            var errorList = new List<string>();
            if(price== 0)
            {
                errorList.Add("価格");
            }

            if (string.IsNullOrEmpty(book.productTitle))
            {
                errorList.Add("本のタイトル");
            }

            if (string.IsNullOrEmpty(book.publisher))
            {
                errorList.Add("出版社");
            }

            if (string.IsNullOrEmpty(book.author))
            {
                errorList.Add("著者");
            }
            if (string.IsNullOrEmpty(book.isbn13))
            {
                errorList.Add("ISBN");
            }

            var text = book.productTitle + "\t"+ book.publisher + "\t"+book.author+ "\t"+ price +"\t"+"\t"+"\t"+"\t"+"\t"+"\t"+book.isbn13;
            Clipboard.SetText(text);
            if (errorList.Any())
            {
                var sb = new StringBuilder();
                sb.Append("次ののデータの取得に失敗しました ");
                sb.Append(string.Join(" ", errorList));
                string bookmarklet = $"javascript:(function(){{alert('{sb.ToString()}');}})();";
                webView.CoreWebView2.ExecuteScriptAsync(bookmarklet);
            }
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(url)) 
            {
                string bookmarklet = "javascript:(function(){alert('リンクコピーに失敗しました');})();";
                webView.CoreWebView2.ExecuteScriptAsync(bookmarklet);
            }
            else if(string.IsNullOrEmpty(productTitle)|| string.IsNullOrEmpty(asin))
            {
                var htmlLink = $"<a href=\"{url}\">{title}</a>";
                var titleUrl = $"[{title}]({url})";
                // クリップボードに書き込む
                DataObject dataObject = new DataObject();
                dataObject.SetData(DataFormats.Html, htmlLink);
                dataObject.SetData(DataFormats.Text, titleUrl);
                Clipboard.SetDataObject(dataObject);
                string bookmarklet = "javascript:(function(){alert('リンクをコピーしました');})();";
                webView.CoreWebView2.ExecuteScriptAsync(bookmarklet);

            }
            else 
            {
                var shortURL = $"https://www.amazon.co.jp/dp/{asin}";
                var htmlLink = $"<a href=\"{shortURL}\">{productTitle}</a>";
                var titleUrl = $"[{productTitle}]({shortURL})";

                // クリップボードに書き込む
                var dataObject = new DataObject();
                dataObject.SetData(DataFormats.Html, htmlLink);
                dataObject.SetData(DataFormats.Text, titleUrl);
                Clipboard.SetDataObject(dataObject);
                string bookmarklet = "javascript:(function(){alert('短縮形のリンクをコピーしました');})();";
                webView.CoreWebView2.ExecuteScriptAsync(bookmarklet);
            }

        }

        private void webView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            this.CanExectuteCommand = false;
            OnPropertyChanged(nameof(CanExectuteCommand));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private void Button_Click3(object sender, RoutedEventArgs e)
        {
            // デフォルトのブラウザでURLを開く
            Process.Start(new ProcessStartInfo
            {
                FileName = this.url,
                UseShellExecute = true
            });
        }

        private static int ConvertStringToInt(string input)
        {
            // 円記号とカンマを削除
            var cleanedString = input.Replace("￥", "").Replace("\\", "").Replace(",", "").Trim();

            // 整数に変換
            if (int.TryParse(cleanedString, out int result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }

        private async Task<BookInfo> FetchBookInfoAsync()
        {
string script = @"
var productTitleElement = document.getElementById('productTitle');
if(!productTitleElement)var productTitleElement = document.getElementById('ebooksProductTitle');
var productTitle = productTitleElement ? productTitleElement.innerText.trim() : '';

var authorElement = document.querySelector('.author.notFaded a');
var author = authorElement ? authorElement.innerText.trim() : '';

var ASINElement = document.querySelector('#ASIN');
var ASIN = ASINElement ? ASINElement.value : '';

var ISBNElement = document.querySelector('.a-section#rpi-attribute-book_details-isbn13 .a-section.rpi-attribute-value span');
var ISBN = ISBNElement ? ISBNElement.innerText.trim() : '';

var priceWholeElement = document.querySelector('.a-price-whole');
var price = priceWholeElement ? priceWholeElement.innerText.trim() : '';

var publisherElement = document.querySelector('#rpi-attribute-book_details-publisher .rpi-attribute-value span');
var publisher = publisherElement ? publisherElement.innerText.trim() : '';

JSON.stringify({ productTitle:productTitle, isbn13: ISBN, price: price, publisher: publisher, author:author, asin:ASIN });
";

            try
            {
                string resultString = await webView.CoreWebView2.ExecuteScriptAsync(script);
                resultString = resultString.Trim('"').Replace("\\\"", "\"");
                var result = JsonConvert.DeserializeObject<BookInfo>(resultString);

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error fetching book info: " + ex.Message);
                return null;
            }
        }

        public class BookInfo
        {
            [JsonProperty("productTitle")]
            public string productTitle { get; set; }
            [JsonProperty("isbn13")]
            public string isbn13 { get; set; }

            [JsonProperty("asin")]
            public string ASIN { get; set; }
            [JsonProperty("author")]
            public string author { get; set; }

            [JsonProperty("price")]
            public string price { get; set; }
            [JsonProperty("publisher")]
            public string publisher { get; set; }
        }
    }
}
