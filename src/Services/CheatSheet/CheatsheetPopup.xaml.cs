using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace CheatSheet
{
    /// <summary>
    /// CheatSheetWindow.xaml の相互作用ロジック
    /// </summary>
public partial  class CheatsheetPopup : Window
    {
        public CheatsheetPopup(string content, string title)
        {
            Title = $"{title} Shortcut Keys Cheatsheet";
            Width = 250;
            Height = SystemParameters.PrimaryScreenHeight;
            ShowInTaskbar = false;
            Topmost = true;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = System.Windows.Media.Brushes.Transparent; // 背景を透明に設定

            // ツールウィンドウとして設定
            ShowActivated = false;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = SystemParameters.PrimaryScreenWidth - Width;
            Top = 0;

            // コンテンツを設定
            Border border = new Border
            {
                Background = System.Windows.Media.Brushes.Black,
                Opacity = 0.5, // 少し透明に
                Child = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = content,
                        Foreground = System.Windows.Media.Brushes.White,
                        TextWrapping = TextWrapping.Wrap,
                        Opacity = 1,
                        Margin = new Thickness(10)
                    }
                }
            };

            Content = border;

            // ウィンドウの拡張スタイルを設定
            Loaded += (sender, e) =>
            {
                WindowInteropHelper wih = new WindowInteropHelper(this);
                int exStyle = GetWindowLong(wih.Handle, GWL_EXSTYLE);
                exStyle |= WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE | WS_EX_TRANSPARENT;
                SetWindowLong(wih.Handle, GWL_EXSTYLE, exStyle);
            };
        }

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
