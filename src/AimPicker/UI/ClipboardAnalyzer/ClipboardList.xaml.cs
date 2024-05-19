using AimPicker.Service.Clipboard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AimPicker.UI.ClipboardAnalyzer
{
    /// <summary>
    /// ClipboardList.xaml の相互作用ロジック
    /// </summary>
    public partial class ClipboardList : System.Windows.Controls.UserControl
    {
        public ObservableCollection<ClipboardService.IClipboardData> Items { get; set; } = new ObservableCollection<ClipboardService.IClipboardData>();
        public ClipboardList()
        {
            InitializeComponent();
            var  clipboardDatas  = ClipboardService.Load();
            foreach (var clipboardData in clipboardDatas)
            {
                if (clipboardData.IsDisabled)
                {
                    continue;
                }

                Items.Add(clipboardData);
            }
            this.DataContext = this;
        }
    }
}
