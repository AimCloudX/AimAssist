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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ClipboardAnalyzer.DomainModels;
using ClipboardAnalyzer.Services;

namespace ClipboardAnalyzer
{
    /// <summary>
    /// ClipboardList.xaml の相互作用ロジック
    /// </summary>
    public partial class ClipboardList : UserControl
    {
        public ObservableCollection<IClipboardData> Items { get; set; } = new ObservableCollection<IClipboardData>();
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

            var formats = Items.Select(x => x.Format).ToList();
            formats.Add("JSON");

            this.ComboBox.ItemsSource = formats;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(this.PreviewText.Text);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedText = this.ComboBox.SelectedItem as string;

            foreach(var item in Items)
            {
                if(item.Format == selectedText)
                {
                    this.PreviewText.Text = item.Data.ToString();
                }
            }

            if(selectedText == "JSON")
            {
                string text = "";
                foreach (var item in Items)
                {
                    if (item.Format == "Text")
                    {
                        text = item.Data.ToString();
                    }
                }

                if(string.IsNullOrEmpty(text))
                {
                    return;
                }

                string[] result = text.Split('\t');
                if (result.Length == 2)
                {
                    var key = result[0];
                    var value = result[1].Replace("\r\n", "");
                    this.PreviewText.Text = $"{{\"{key}\" : \"{value}\"}}";
                }

                
            }
        }
    }
}
