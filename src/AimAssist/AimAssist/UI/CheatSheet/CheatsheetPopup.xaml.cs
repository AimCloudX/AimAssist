﻿using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace AimAssist.UI.CheatSheet
{
    /// <summary>
    /// CheatSheetWindow.xaml の相互作用ロジック
    /// </summary>
public partial class CheatsheetPopup : Window
    {
        public CheatsheetPopup(string content, string title)
        {
            InitializeComponent();
            ShowInTaskbar = false;
            Topmost = true;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = System.Windows.Media.Brushes.Transparent;

            // ツールウィンドウとして設定
            ShowActivated = false;
            WindowStartupLocation = WindowStartupLocation.Manual;

            Border titleBorder = new Border
            {
                Background = System.Windows.Media.Brushes.Black,
                Opacity = 0.8,
                Child = new TextBlock()
                {
                    Text = title,
                    Foreground = System.Windows.Media.Brushes.White,
                }
            };

            TitleGrid.Children.Add(titleBorder);

            // コンテンツを設定
            Border border = new Border
            {
                Background = System.Windows.Media.Brushes.Black,
                Opacity = 0.8,
                Child = CreateMultiColumnContent(content)
            };

            SheetGrid.Children.Add(border);

            // ウィンドウの拡張スタイルを設定
            Loaded += (sender, e) =>
            {
                WindowInteropHelper wih = new WindowInteropHelper(this);
                int exStyle = GetWindowLong(wih.Handle, GWL_EXSTYLE);
                exStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT;
                SetWindowLong(wih.Handle, GWL_EXSTYLE, exStyle);
            };
        }

        private UIElement CreateMultiColumnContent(string content)
        {
            var sections = content.Split(new[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
            var grid = new Grid();

            for (int i = 0; i < sections.Length; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                var column = new StackPanel { Margin = new Thickness(10) };

                var lines = sections[i].Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                // ヘッダーを追加
                if (lines.Length > 0)
                {
                    column.Children.Add(new TextBlock
                    {
                        Text = lines[0].Trim(),
                        FontWeight = FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.Yellow,
                        Margin = new Thickness(0, 0, 0, 10)
                    });
                }

                // 内容を追加
                for (int j = 1; j < lines.Length; j++)
                {
                    var item = lines[j].Trim();
                    if (item.StartsWith("-"))
                    {
                        item = item.Substring(1).Trim();
                    }
                    column.Children.Add(new TextBlock
                    {
                        Text = item,
                        Foreground = System.Windows.Media.Brushes.White,
                        Margin = new Thickness(0, 0, 0, 5)
                    });
                }

                Grid.SetColumn(column, i);
                grid.Children.Add(column);
            }

            return grid;
        }
        //private UIElement CreateMultiColumnContent(string content)
        //{
        //    var lines = content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        //    int itemsPerColumn = (int)Math.Ceiling(lines.Length / 8.0);  

        //    var grid = new Grid();
        //    grid.ColumnDefinitions.Add(new ColumnDefinition());
        //    grid.ColumnDefinitions.Add(new ColumnDefinition());
        //    grid.ColumnDefinitions.Add(new ColumnDefinition());

        //    grid.ColumnDefinitions.Add(new ColumnDefinition());
        //    grid.ColumnDefinitions.Add(new ColumnDefinition());
        //    grid.ColumnDefinitions.Add(new ColumnDefinition());
        //    grid.ColumnDefinitions.Add(new ColumnDefinition());
        //    grid.ColumnDefinitions.Add(new ColumnDefinition());

        //    for (int i = 0; i < 8; i++)
        //    {
        //        var column = new StackPanel { Margin = new Thickness(10) };
        //        var columnItems = lines.Skip(i * itemsPerColumn).Take(itemsPerColumn);
        //        foreach (var item in columnItems)
        //        {
        //            column.Children.Add(new TextBlock { Text = item, 
        //                Foreground = System.Windows.Media.Brushes.White,
        //                Margin = new Thickness(0, 0, 0, 5) });
        //        }
        //        Grid.SetColumn(column, i);
        //        grid.Children.Add(column);
        //    }

        //    return grid;
        //}

        protected override void OnDeactivated(EventArgs e)
        {
            base.OnDeactivated(e);
            Topmost = true;
        }

        // Win32 API 呼び出し
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int WS_EX_TRANSPARENT = 0x00000020;

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
