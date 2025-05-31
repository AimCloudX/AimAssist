using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace AimAssist.Units.Implementation.CodeGenarator
{
    /// <summary>
    /// ApiKeyInputWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ApiKeyInputWindow : Window
    {
        public string ApiKey { get; private set; }

        public ApiKeyInputWindow()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ApiKey = ApiKeyTextBox.Text.Trim();
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                // デフォルトのブラウザでリンクを開く
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"リンクを開く際にエラーが発生しました: {ex.Message}");
            }
            e.Handled = true;
        }
    }
}
