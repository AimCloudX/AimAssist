using Markdig;
using Microsoft.Web.WebView2.Core;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace AimAssist.UI.Combos
{
    /// <summary>
    /// MarkdownView.xaml の相互作用ロジック
    /// </summary>
    public partial class MarkdownView : System.Windows.Controls.UserControl
    {
        private string markdownText;

        public MarkdownView(string filePath)
        {
            InitializeComponent();
            if (File.Exists(filePath))
            {
                markdownText = File.ReadAllText(filePath);
            }

            // WebView2の初期化完了イベントを設定
            WebView.NavigationCompleted += OnNavigationCompleted;
            // WebView2を初期化
            InitializeAsync();
        }
        public MarkdownView(IEnumerable<string> filePaths)
        {
            InitializeComponent();
            var sb = new StringBuilder();
            foreach (string filePath in filePaths)
            {
                sb.AppendLine(File.ReadAllText(filePath));
            }

            markdownText = sb.ToString() ;

            // WebView2の初期化完了イベントを設定
            WebView.NavigationCompleted += OnNavigationCompleted;
            // WebView2を初期化
            InitializeAsync();
        }

        public MarkdownView()
        {
        }

        private async void InitializeAsync()
        {
            await WebView.EnsureCoreWebView2Async(null);

            // MarkdownをHTMLに変換し、目次を作成
            string htmlText = MarkdownToHtmlWithAnchors(markdownText);

            // CSSを取得
            string css = GetNordThemeCss();

            // HTMLにエンコーディングを指定するメタタグとNordテーマのCSSを追加
            string fullHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <style>
        {css}
    </style>
</head>
<body>
    {htmlText}
    <script>
        function scrollToElement(id) {{
            var element = document.getElementById(id);
            if (element) {{
                element.scrollIntoView({{ behavior: 'smooth', block: 'start' }});
            }}
        }}
    </script>
</body>
</html>";

            // WebView2にHTMLを表示
            WebView.NavigateToString(fullHtml);
            // Navigatingイベントを設定
        }


   private void WebBrowser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.Uri != null && (e.Uri.Scheme == Uri.UriSchemeHttp || e.Uri.Scheme == Uri.UriSchemeHttps))
            {
                e.Cancel = true;
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
        }
        private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            // 目次を作成してTreeViewに追加
            var outline = CreateOutline(markdownText);
            foreach (var item in outline)
            {
                OutlineTreeView.Items.Add(item);
            }

            // TreeViewItemのクリックイベントを設定
            SetTreeViewItemClickEvent(OutlineTreeView.Items);
        }
        private string MarkdownToHtmlWithAnchors(string markdownText)
        {
            var lines = markdownText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int headingCounter = 0;
            var modifiedLines = new List<string>();

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                {
                    headingCounter++;
                    var level = line.TakeWhile(c => c == '#').Count();
                    var text = line.Substring(level).Trim();
                    var anchor = $"<a id='heading{headingCounter}'></a>";
                    modifiedLines.Add($"{anchor}<h{level}>{text}</h{level}>");
                }
                else
                {
                    modifiedLines.Add(line);
                }
            }

            var modifiedMarkdown = string.Join(Environment.NewLine, modifiedLines);
            return Markdown.ToHtml(modifiedMarkdown);
        }

        private List<TreeViewItem> CreateOutline(string markdownText)
        {
            var outline = new List<TreeViewItem>();
            var lines = markdownText.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int headingCounter = 0;

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                {
                    headingCounter++;
                    var level = line.TakeWhile(c => c == '#').Count();
                    var text = line.Substring(level).Trim();
                    var item = new TreeViewItem { Header = text, Tag = $"heading{headingCounter}" };
                    if(level < 2)
                    {
                        item.IsExpanded = true;
                    }

                    if (level == 1)
                    {
                        outline.Add(item);
                    }
                    else
                    {
                        var parent = FindParent(outline, level - 1);
                        parent?.Items.Add(item);
                    }
                }
            }

            return outline;
        }

        private TreeViewItem FindParent(List<TreeViewItem> items, int level)
        {
            TreeViewItem parent = null;
            foreach (var item in items)
            {
                if (level == 1)
                {
                    parent = item;
                }
                else
                {
                    parent = FindParent(item.Items.OfType<TreeViewItem>().ToList(), level - 1);
                }
            }
            return parent;
        }

        private void SetTreeViewItemClickEvent(ItemCollection items)
        {
            foreach (var item in items.OfType<TreeViewItem>())
            {
                item.MouseDoubleClick += (s, e) =>
                {
                    var treeViewItem = s as TreeViewItem;
                    if (treeViewItem != null)
                    {
                        var id = treeViewItem.Tag.ToString();
                        WebView.CoreWebView2.ExecuteScriptAsync($"scrollToElement('{id}')");
                    }
                };

                // 再帰的に子アイテムにも設定
                if (item.Items.Count > 0)
                {
                    SetTreeViewItemClickEvent(item.Items);
                }
            }
        }

        private string GetNordThemeCss()
        {
            return @"
        body {
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #2E3440;
            color: #D8DEE9;
            margin: 0;
            padding: 20px;
        }

        h1, h2, h3, h4, h5, h6 {
            color: #88C0D0;
        }

        a {
            color: #81A1C1;
            text-decoration: none;
        }

        a:hover {
            text-decoration: underline;
        }

        pre {
            background-color: #3B4252;
            padding: 10px;
            border-radius: 5px;
            overflow-x: auto;
        }

        code {
            background-color: #4C566A;
            padding: 2px 4px;
            border-radius: 3px;
        }

        blockquote {
            border-left: 4px solid #81A1C1;
            padding-left: 10px;
            color: #ECEFF4;
        }

        ul, ol {
            padding-left: 20px;
        }

        table {
            width: 100%;
            border-collapse: collapse;
            margin-bottom: 20px;
        }

        th, td {
            padding: 10px;
            border: 1px solid #4C566A;
        }

        th {
            background-color: #3B4252;
        }

        td {
            background-color: #2E3440;
        }
    ";
        }

        private void WebView_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            // 外部リンクをチェック（httpやhttpsで始まるリンク）
            if (e.Uri.StartsWith("http://") || e.Uri.StartsWith("https://"))
            {
                e.Cancel = true; // WebView2でのナビゲーションをキャンセル
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.Uri,
                    UseShellExecute = true
                }); // 既定のブラウザでURLを開く
            }
        }
    }
}

