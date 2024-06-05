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
            UpdateClipboard();
        }

        private void UpdateClipboard()
        {
            var clipboardDatas = ClipboardService.Load();
            Items.Clear();
            foreach (var clipboardData in clipboardDatas)
            {
                if (clipboardData.IsDisabled)
                {
                    continue;
                }

                Items.Add(clipboardData);
            }
            this.DataContext = this;

            var formats = Items.Where(x => x.Format != "PNG").Select(x => x.Format).ToList();

            this.ComboBox.ItemsSource = formats;
            this.ComboBox.SelectedItem = "Text";
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var selectedFormat = this.ComboBox.SelectedItem as string;
            Clipboard.SetData(selectedFormat, this.PreviewText.Text);
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetPreviewText();
        }

        private void SetPreviewText()
        {
            var selectedFormat = this.ComboBox.SelectedItem as string;

            if (string.IsNullOrEmpty(selectedFormat))
            {
                this.PreviewText.Text = string.Empty;
                return;
            }

            var foramtData = Items.FirstOrDefault(x => x.Format == selectedFormat);
            if (foramtData != null)
            {
                this.PreviewText.Text = foramtData.Data.ToString();
            }
        }

        private void JsonConvert_Click(object sender, RoutedEventArgs e)
        {
                var text = this.PreviewText.Text;
                if(string.IsNullOrEmpty(text))
                {
                    return;
                }

                var separators = new string[] { "\r\n", "\n" };
                var lines = text.Split(separators, StringSplitOptions.None);

                var sb  = new StringBuilder();
                foreach( var line in lines )
                {
                    var splitedText = line.Split(new char[] { ' ', '\t' });
                    var key  = splitedText[0];
                var value = string.Join(" ", splitedText, 1, splitedText.Length - 1);

                    sb.AppendLine($"{{\"{key}\" : \"{value}\"}}");
                }

                this.PreviewText.Text = sb.ToString().TrimEnd('\r', '\n');
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            SetPreviewText();
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            UpdateClipboard();
            SetPreviewText();
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            var text = this.PreviewText.Text;
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var replacedText = text.Replace(this.beforeText.Text, this.afterText.Text);
            this.PreviewText.Text = replacedText;


        }
    }
}
