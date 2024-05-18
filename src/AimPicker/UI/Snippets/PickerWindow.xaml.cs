using AimPicker.Domain;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace AimPicker.UI.Tools.Snippets
{
    public partial class PickerWindow : Window
    {
        private bool isClosing;

        DispatcherTimer? typingTimer;
        private string beforeText = string.Empty;

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

        public ObservableCollection<Combo> ComboLists { get; } = new ObservableCollection<Combo>()
                                                                      {
            new Combo("aim","AimNext"),
            new Combo("Today",DateTime.Now.ToString("d")),
            new Combo("Now",DateTime.Now.ToString("t")),
            new Combo("AppData",Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
            new Combo("Downloads",Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads")),
            new Combo("環境変数","control.exe sysdm.cpl,,3"),
                                                                      };

        public string SnippetText { get; set; } = string.Empty;
        public PickerMode Mode { get; }

        private void CloseWindow()
        {
            if (this.isClosing)
            {
                return;
            }

            this.isClosing = true;
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
            if(this.typingTimer != null)
            {
                this.typingTimer.Tick -= this.HandleTypingTimerTimeout;
            }

            this.typingTimer = null;
            if (this.beforeText.Equals(this.FilterTextBox.Text))
            {
                return;
            }

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.ComboLists);
            view.Filter = this.Filter;
            this.typingTimer = null;
            this.beforeText = this.FilterTextBox.Text;
            this.ComboListBox.SelectedIndex = 0;
        }

        private bool Filter(object obj)
        {
            if (string.IsNullOrEmpty(this.FilterTextBox.Text))
            {
                return true;
            }

            var combo = obj as Combo;
            if (combo != null)
            {
                if (!combo.Name.Contains(this.FilterTextBox.Text, StringComparison.OrdinalIgnoreCase))
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
                if (this.ComboListBox.SelectedItem is Combo combo)
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
    }
}