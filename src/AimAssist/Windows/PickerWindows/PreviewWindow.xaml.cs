using AimAssist.WebViewCash;
using ModernWpf.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AimAssist.UI
{
    /// <summary>
    /// PreviewWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class PreviewWindow : Window
    {
        public PreviewWindow()
        {
            InitializeComponent();
        }

        private void Winodw_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Contents.Children.Clear();
            UIElementRepository.PreviewWindow = null;
        }

        private void Winodw_ContentRendered(object sender, EventArgs e)
        {
        }
    }
}
