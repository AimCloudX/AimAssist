using AimPicker.Domain;
using AimPicker.UI.ClipboardAnalyzer;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace AimPicker.UI.Tools.Snippets
{
    public partial class PickerWindow : Window, INotifyPropertyChanged
    {
        private bool isClosing;

        DispatcherTimer? typingTimer;
        private string beforeText = string.Empty;
        private PreviewWindow previewWindow;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PickerWindow(PickerMode mode) : this()
        {
            switch (mode)
            {
                case PickerMode.Snippet:
                    break;
                case PickerMode.Command:
                    this.FilterTextBox.Text = ">";
                    break;
                case PickerMode.Calculate:
                    this.FilterTextBox.Text = "=";
                    break;
            }

            Mode = mode;

            this.FilterTextBox.Focus();
            this.FilterTextBox.SelectionStart = this.FilterTextBox.Text.Length;
            this.ComboListBox.SelectedIndex = 0;
        }

        public PickerWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public Dictionary<PickerMode, ObservableCollection<ICombo>> ComboDictionary { get; private set; } = new Dictionary<PickerMode, ObservableCollection<ICombo>>()
        {
            {PickerMode.Snippet, new ObservableCollection<ICombo>()
            {
            new PickerSnippet("aim","AimNext"),
            new PickerSnippet("Today",DateTime.Now.ToString("d")),
            new PickerSnippet("Now",DateTime.Now.ToString("t")),
            new PickerSnippet("AppData",Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
            new PickerSnippet("Downloads",Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads")),
            new PickerSnippet("環境変数","control.exe sysdm.cpl,,3"),
            } },
            { PickerMode.Command ,new ObservableCollection<ICombo>()
            {
                new PickerCommand("ClipboardAnalyzer",System.Windows.Clipboard.GetText(), ()=>new ClipboardList())
            } },
            {PickerMode.Calculate, new ObservableCollection<ICombo>() }

        };


        public ObservableCollection<ICombo> ComboLists => this.ComboDictionary[this.Mode];

        public string SnippetText { get; set; } = string.Empty;
        public PickerMode Mode { get; set; }

        private void CloseWindow()
        {
            if (this.isClosing)
            {
                return;
            }

            this.Close();
        }

        private void CommandPaletteWindow_OnDeactivated(object sender, EventArgs e)
        {
#if DEBUG
            return;
#endif

            this.CloseWindow();
        }

        private void HandleTypingTimerTimeout(object sender, EventArgs e)
        {
            if (this.typingTimer != null)
            {
                this.typingTimer.Tick -= this.HandleTypingTimerTimeout;
            }

            this.typingTimer = null;
            if (this.beforeText.Equals(this.FilterTextBox.Text))
            {
                return;
            }

            if (this.FilterTextBox.Text.StartsWith('>'))
            {
                this.Mode = PickerMode.Command;
            }else if (this.FilterTextBox.Text.StartsWith('='))
            {
                this.Mode = PickerMode.Calculate;
            }
            else
            {
                this.Mode = PickerMode.Snippet;
            }

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.ComboLists);
            view.Filter = this.Filter;
            this.typingTimer = null;
            this.beforeText = this.FilterTextBox.Text;
            this.OnPropertyChanged(nameof(this.ComboLists));
            this.ComboListBox.SelectedIndex = 0;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private bool Filter(object obj)
        {
            var filterText = this.FilterTextBox.Text;
            if(this.Mode != PickerMode.Snippet)
            {
                filterText = filterText.Substring(1);
            }

            if (string.IsNullOrEmpty(filterText))
            {
                return true;
            }

            var combo = obj as PickerSnippet;
            if (combo != null)
            {
                if (!combo.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private void SnippetToolWindow_OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (this.ComboListBox.SelectedItem is PickerSnippet combo)
                {
                    this.SnippetText = combo.Snippet;
                }

                this.CloseWindow();
            }
            else if (e.Key == Key.Escape)
            {
                this.CloseWindow();
            }
        }

        private void TextBox_OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                var index = this.ComboListBox.SelectedIndex;
                if (index <= 0) return;
                this.ComboListBox.SelectedIndex = index - 1;
            }

            if (e.Key == Key.Down)
            {
                var index = this.ComboListBox.SelectedIndex;
                if (index >= this.ComboListBox.Items.Count) return;
                this.ComboListBox.SelectedIndex = index + 1;
            }

            this.ComboListBox.ScrollIntoView(this.ComboListBox.SelectedItem);
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.typingTimer == null)
            {
                this.typingTimer = new DispatcherTimer
                {
                    Interval = default,
                    IsEnabled = false,
                    Tag = null
                };

                this.typingTimer.Interval = TimeSpan.FromMilliseconds(100);

                this.typingTimer.Tick += this.HandleTypingTimerTimeout;
            }

            this.typingTimer.Stop(); // Resets the timer
            this.typingTimer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 新しいウィンドウを作成
            this.previewWindow = new PreviewWindow();

            // MainWindowの位置とサイズを取得
            double mainWindowLeft = this.Left;
            double mainWindowTop = this.Top;
            double mainWindowWidth = this.Width;
            double mainWindowHeight = this.Height;

            // SecondaryWindowの位置を設定
            previewWindow.Left = mainWindowLeft + mainWindowWidth;
            previewWindow.Top = mainWindowTop + mainWindowHeight + 130;
            //secondaryWindow.Top = mainWindowTop;

            // SecondaryWindowを表示
            previewWindow.Show();

        }

        private void ComboListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(this.previewWindow == null)
            {
                return;
            }

            if(this.ComboListBox.SelectedItem is ICombo combo)
            {
                this.previewWindow.Contents?.Children.Clear();

                var uiElement = combo.Factory.Invoke();
                this.previewWindow?.Contents?.Children.Add(uiElement);
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.previewWindow?.Close();
            this.isClosing = true;
        }
    }
}