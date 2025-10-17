using System.IO;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;

namespace Common.UI.Markdown
{
    public partial class MarkdownView
    {
        private readonly string markdownText = string.Empty;

        public MarkdownView(string filePath)
        {
            InitializeComponent();
            if (File.Exists(filePath))
            {
                markdownText = File.ReadAllText(filePath);
            }

            WebView.NavigationCompleted += OnNavigationCompleted;
            InitializeAsync();
        }

        public MarkdownView()
        {
        }

        private async void InitializeAsync()
        {
            await WebView.EnsureCoreWebView2Async(null);

            var htmlText = MarkdownToHtmlWithAnchors(markdownText);

            var css = GetNordThemeCss();

            var fullHtml = $$"""

                             <!DOCTYPE html>
                             <html>
                             <head>
                                 <meta charset="UTF-8">
                                 <style>
                                     {{css}}
                                 </style>
                             </head>
                             <body>
                                 {{htmlText}}
                                 <script>
                                     function scrollToElement(id) {
                                         var element = document.getElementById(id);
                                         if (element) {
                                             element.scrollIntoView({ behavior: 'smooth', block: 'start' });
                                         }
                                     }
                                 </script>
                             </body>
                             </html>
                             """;

            WebView.NavigateToString(fullHtml);
        }

        private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            var outline = CreateOutline(markdownText);
            foreach (var item in outline)
            {
                OutlineTreeView.Items.Add(item);
            }

            SetTreeViewItemClickEvent(OutlineTreeView.Items);
        }

        private static string MarkdownToHtmlWithAnchors(string markdownText)
        {
            var lines = markdownText.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
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
            return modifiedMarkdown;
            // return Markdig.Markdown.ToHtml(modifiedMarkdown);
        }

        private List<TreeViewItem> CreateOutline(string markdown)
        {
            var outline = new List<TreeViewItem>();
            var lines = markdown.Split(new[] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
            var headingCounter = 0;

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                {
                    headingCounter++;
                    var level = line.TakeWhile(c => c == '#').Count();
                    var text = line.Substring(level).Trim();
                    var item = new TreeViewItem {Header = text, Tag = $"heading{headingCounter}"};
                    if (level < 2)
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

        private static TreeViewItem? FindParent(List<TreeViewItem> items, int level)
        {
            TreeViewItem? parent = null;
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
                item.MouseDoubleClick += (s, _) =>
                {
                    if (s is not TreeViewItem treeViewItem) return;
                    var id = treeViewItem.Tag?.ToString();
                    if (!string.IsNullOrEmpty(id))
                    {
                        WebView.CoreWebView2?.ExecuteScriptAsync($"scrollToElement('{id}')");
                    }
                };

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

        private void WebView_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (e.Uri.StartsWith("http://") || e.Uri.StartsWith("https://"))
            {
                e.Cancel = true;
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = e.Uri,
                    UseShellExecute = true
                });
            }
        }
    }
}