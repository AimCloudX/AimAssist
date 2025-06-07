using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AimAssist.Core.Attributes;
using AimAssist.Core.Interfaces;
using AimAssist.Services.ClipboardAnalyzer;
using AimAssist.Services.ClipboardAnalyzer.DomainModels;
using AimAssist.Units.Implementation.ClipboardAnalyzer;

namespace AimAssist.Units.Implementation.ClipboardAnalyzer.UI
{
    [AutoDataTemplate(typeof(ClipboardUnit), useDependencyInjection: true)]
    public partial class ClipboardList
    {
        public ObservableCollection<IClipboardData> Items { get; set; } = [];

        public ClipboardList(IEditorOptionService? editorOptionService)
        {
            InitializeComponent();
            UpdateClipboard();
            if (editorOptionService != null)
            {
                this.editor.SetOption(editorOptionService.Option);
            }
        }

        public ClipboardList() : this(null)
        {
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
                selectedFormat = "Text";
            }

            Clipboard.SetData(selectedFormat, await this.editor.GetText() ?? string.Empty);
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
                _ = this.editor.SetTextAsync(string.Empty);
                return;
            }

            var foramtData = Items.FirstOrDefault(x => x.Format == selectedFormat);
            if (foramtData?.Data != null)
            {
                _ = this.editor.SetTextAsync(foramtData.Data.ToString() ?? string.Empty);
            }
        }

        private async void JsonConvert_Click(object sender, RoutedEventArgs e)
        {
            var text = await this.editor.GetText();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var separators = new[] { "\r\n", "\n" };
            var lines = text.Split(separators, StringSplitOptions.None);

            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    sb.AppendLine();
                    continue;
                }

                var splitedText = line.Split(' ', '\t');
                var key = splitedText[0];
                var value = string.Join(" ", splitedText, 1, splitedText.Length - 1);

                sb.AppendLine($"{{\"{key}\" : \"{value}\"}}");
            }

            await this.editor.SetTextAsync(sb.ToString().TrimEnd('\r', '\n'));
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
    }
}
