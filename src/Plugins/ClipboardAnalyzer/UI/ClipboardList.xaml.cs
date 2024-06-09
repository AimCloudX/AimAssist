using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AimAssist.Core.Options;
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
            this.editor.SetOption(EditorOptionService.Option);
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var selectedFormat = this.ComboBox.SelectedItem as string;
            if(selectedFormat == null)
            {
                selectedFormat = "Text"; ;
            }

            Clipboard.SetData(selectedFormat, await this.editor.GetText());
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
                this.editor.SetText(string.Empty);
                return;
            }

            var foramtData = Items.FirstOrDefault(x => x.Format == selectedFormat);
            if (foramtData != null)
            {
                this.editor.SetText(foramtData.Data.ToString());
            }
        }

        private async void JsonConvert_Click(object sender, RoutedEventArgs e)
        {
            var text = await this.editor.GetText();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var separators = new string[] { "\r\n", "\n" };
            var lines = text.Split(separators, StringSplitOptions.None);

            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    sb.AppendLine();
                    continue;
                }

                var splitedText = line.Split(new char[] { ' ', '\t' });
                var key = splitedText[0];
                var value = string.Join(" ", splitedText, 1, splitedText.Length - 1);

                sb.AppendLine($"{{\"{key}\" : \"{value}\"}}");
            }

            this.editor.SetText(sb.ToString().TrimEnd('\r', '\n'));
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

        private async void Convert_Click(object sender, RoutedEventArgs e)
        {
            var text = await this.editor.GetText();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var replacedText = text.Replace(this.beforeText.Text, this.afterText.Text);
            this.editor.SetText(replacedText);
        }
    }
}
