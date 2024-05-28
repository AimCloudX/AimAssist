using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace AimPicker.UI.Combos.Commands
{
    public partial class AmazonWebViewControl : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        private string url;
        private string readedURl;
        private string title;

        private string producttitle;
        private string ISBN;
        private int Price;
        private string Publisher;
        private string Author;
        private string ASIN;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool CanExectuteCommand { get; set; }

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
            var dataObject = new System.Windows.DataObject();
            dataObject.SetData(System.Windows.DataFormats.Html, htmlLink);
            dataObject.SetData(System.Windows.DataFormats.Text, titleUrl);
            System.Windows.Clipboard.SetDataObject(dataObject);

            string bookmarklet = "javascript:(function(){alert('リンクをコピーしました');})();";
            webView.CoreWebView2.ExecuteScriptAsync(bookmarklet);
        }

        private void webView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // ナビゲーションが成功したか確認
            if (e.IsSuccess)
            {
                // 現在のURLを取得
                readedURl = webView.Source.ToString();

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
                    // JavaScriptの結果を取得
                    producttitle = task.Result;

                    // JSON形式で返されるため、トリムしてダブルクォーテーションを削除
                    producttitle = producttitle.Trim('"');

                });

var script = @"
var ISBNElement = document.querySelector('.a-section#rpi-attribute-book_details-isbn13 .a-section.rpi-attribute-value span');
var ISBN = ISBNElement ? ISBNElement.innerText.trim() : '';
ISBN;
            ";

                webView.CoreWebView2.ExecuteScriptAsync(script).ContinueWith(task =>
                {
                    // JavaScriptの結果を取得
                    ISBN = task.Result;

                    // JSON形式で返されるため、トリムしてダブルクォーテーションを削除
                    ISBN = ISBN.Trim('"');

                });

                // price
var priceScript = @"
var priceElement = document.querySelector('.a-price .a-offscreen');
var price = priceElement ? priceElement.innerText.trim() : '';
price;
            ";

                webView.CoreWebView2.ExecuteScriptAsync(priceScript).ContinueWith(task =>
                {
                    // JavaScriptの結果を取得
                    var  priceString = task.Result;

                    // JSON形式で返されるため、トリムしてダブルクォーテーションを削除
                    priceString = priceString.Trim('"');

                    Price = ConvertStringToInt(priceString);

                });

                // publisher
string publicherScript = @"
var publisherElement = document.querySelector('.a-section#rpi-attribute-book_details-publisher .a-section.rpi-attribute-value span');
var publisher = publisherElement ? publisherElement.innerText.trim() : '';
publisher;
";

                webView.CoreWebView2.ExecuteScriptAsync(publicherScript).ContinueWith(task =>
                {
                    // JavaScriptの結果を取得
                    Publisher = task.Result;

                    // JSON形式で返されるため、トリムしてダブルクォーテーションを削除
                    Publisher = Publisher.Trim('"');

                });

                // Author
string AuthorScript = @"
var authorElement = document.querySelector('.author.notFaded a');
var author = authorElement ? authorElement.innerText.trim() : '';
author;
";

                webView.CoreWebView2.ExecuteScriptAsync(AuthorScript).ContinueWith(task =>
                {
                    // JavaScriptの結果を取得
                    Author = task.Result;

                    // JSON形式で返されるため、トリムしてダブルクォーテーションを削除
                    Author = Author.Trim('"');

                });

                // ASIN
string asinScript = @"
var ASINElement = document.querySelector('#ASIN');
var ASIN = ASINElement ? ASINElement.value : '';
ASIN;
";

                webView.CoreWebView2.ExecuteScriptAsync(asinScript).ContinueWith(task =>
                {
                    // JavaScriptの結果を取得
                    ASIN = task.Result;

                    // JSON形式で返されるため、トリムしてダブルクォーテーションを削除
                    ASIN = ASIN.Trim('"');

                });


                this.CanExectuteCommand = true;
                OnPropertyChanged(nameof(CanExectuteCommand));

            }
            else
            {
            }
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            var text = producttitle + "\t"+ Publisher + "\t"+Author+ "\t"+Price +"\t"+"\t"+"\t"+"\t"+"\t"+"\t"+ISBN;

            System.Windows.Clipboard.SetText(text);
        }

        private void Button_Click2(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(url)) 
            {
                string bookmarklet = "javascript:(function(){alert('リンクコピーに失敗しました');})();";
                webView.CoreWebView2.ExecuteScriptAsync(bookmarklet);
            }
            else if(string.IsNullOrEmpty(producttitle)|| string.IsNullOrEmpty(ASIN))
            {
                var htmlLink = $"<a href=\"{url}\">{title}</a>";
                var titleUrl = $"[{title}]({url})";
                // クリップボードに書き込む
                var dataObject = new System.Windows.DataObject();
                dataObject.SetData(System.Windows.DataFormats.Html, htmlLink);
                dataObject.SetData(System.Windows.DataFormats.Text, titleUrl);
                System.Windows.Clipboard.SetDataObject(dataObject);
                string bookmarklet = "javascript:(function(){alert('リンクをコピーしました');})();";
                webView.CoreWebView2.ExecuteScriptAsync(bookmarklet);

            }
            else 
            {
                // HTMLリンクとMarkdownリンクを生成
                var shortURL = $"https://www.amazon.co.jp/dp/{ASIN}";
                var htmlLink = $"<a href=\"{shortURL}\">{producttitle}</a>";
                var titleUrl = $"[{producttitle}]({shortURL})";

                // クリップボードに書き込む
                var dataObject = new System.Windows.DataObject();
                dataObject.SetData(System.Windows.DataFormats.Html, htmlLink);
                dataObject.SetData(System.Windows.DataFormats.Text, titleUrl);
                System.Windows.Clipboard.SetDataObject(dataObject);
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
            string cleanedString = input.Replace("\\", "").Replace(",", "");

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
    }
}
