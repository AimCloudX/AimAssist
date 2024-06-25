using AimAssist.Combos.Mode.Snippet;
using AimAssist.Service;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Snippets;
using AimAssist.Unit.Implementation.Web.BookSearch;
using AimAssist.Unit.Implementation.Web.Urls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace AimAssist.UI.PickerWindows
{
    public partial class PickerWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<IUnit> UnitLists { get; } = new ObservableCollection<IUnit>();

        public List<IUnitPackage> UsingPackages { get; } = new List<IUnitPackage>();
        public IMode Mode { get; set; }

        public string SnippetText { get; set; } = string.Empty;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private bool Filter(object obj)
        {
            var filterText = this.FilterTextBox.Text;

            if (string.IsNullOrEmpty(filterText))
            {
                return true;
            }
            if (this.Mode == UrlMode.Instance || this.Mode == BookSearchMode.Instance)
            {
                return true;
            }

            var combo = obj as IUnit;
            if (combo != null)
            {

                if (!combo.Name.Contains(filterText, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsClosing { get; set; }

        DispatcherTimer? typingTimer;
        private string beforeText = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PickerWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;

            this.Mode = SnippetMode.Instance;
            this.UpdateCandidate();
            this.FilterTextBox.Focus();

            this.ComboListBox.SelectedIndex = 0;
        }

        private async void UpdateCandidate()
        {
            string inputText;
            inputText = this.FilterTextBox.Text;

            UnitLists.Clear();
            var units = UnitsService.Instnace.CreateUnits(this.Mode, inputText);
            await foreach (var unit in units)
            {
                UnitLists.Add(unit);
            }
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

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.ComboListBox.Items);
            view.Filter = this.Filter;
            this.typingTimer = null;
            this.beforeText = this.FilterTextBox.Text;
            this.OnPropertyChanged(nameof(this.UnitLists));
            this.ComboListBox.SelectedIndex = 0;
        }

        private void Window_OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (this.ComboListBox.SelectedItem is SnippetUnit combo)
                {
                    this.SnippetText = combo.Text;
                    this.CloseWindow();
                }
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

                if (this.Mode == BookSearchMode.Instance)
                {
                    this.typingTimer.Interval = TimeSpan.FromMilliseconds(500);
                }
                else
                {
                    this.typingTimer.Interval = TimeSpan.FromMilliseconds(100);
                }

                this.typingTimer.Tick += this.HandleTypingTimerTimeout;
            }

            this.typingTimer.Stop(); // Resets the timer
            this.typingTimer.Start();
        }

        private void CloseWindow()
        {
            if (this.IsClosing)
            {
                return;
            }

            this.IsClosing = true;
            this.Close();
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.IsClosing = true;
        }

        private void Window_LostFocus(object sender, EventArgs e)
        {
            this.CloseWindow();

        }
    }
}