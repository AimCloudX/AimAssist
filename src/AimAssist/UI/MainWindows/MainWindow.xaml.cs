using AimAssist.Combos.Mode.Snippet;
using AimAssist.Commands;
using AimAssist.Core.Commands;
using AimAssist.Service;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Standard;
using AimAssist.Unit.Implementation.Web.BookSearch;
using AimAssist.Unit.Implementation.Web.Urls;
using AimAssist.WebViewCash;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

namespace AimAssist.UI.MainWindows
{
    public partial class MainWindow : INotifyPropertyChanged
    {
        public ObservableCollection<IUnit> UnitLists { get; } = new ObservableCollection<IUnit>();
        private IPickerMode mode = StandardMode.Instance;

        public List<IUnitPackage> UsingPackages { get; } = new List<IUnitPackage>();
        public IPickerMode Mode
        {
            get { return this.mode; }
            set
            {
                if (value == this.mode)
                {
                    return;
                }

                this.mode = value;
                // CollectionViewSourceの取得
                UpdateGroupDescription();
                OnPropertyChanged();
            }
        }

        private void UpdateGroupDescription()
        {
            CollectionViewSource groupedItems = (CollectionViewSource)this.Resources["GroupedItems"];

            if (groupedItems != null)
            {
                groupedItems.GroupDescriptions.Clear();
                if (this.mode == StandardMode.Instance)
                {
                    groupedItems.GroupDescriptions.Add(new CustomGroupDescription());
                }
                else
                {
                    groupedItems.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                }
            }
        }

        public string SnippetText { get; set; } = string.Empty;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private bool Filter(object obj)
        {
            var filterText = this.FilterTextBox.Text;
            if (filterText.StartsWith(this.mode.Prefix))
            {
                filterText = filterText.Substring(this.mode.Prefix.Length);
            }

            if (string.IsNullOrEmpty(filterText))
            {
                return true;
            }
            if(this.mode== UrlMode.Instance || this.mode == BookSearchMode.Instance)
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
    }

    public partial class MainWindow : Window
    {
        public bool IsClosing { get; set; }

        DispatcherTimer? typingTimer;
        private string beforeText = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;
            
            this.UpdateCandidate();
            this.FilterTextBox.Text = UIElementRepository.RescentText;
            this.FilterTextBox.Focus();
            if(!string.IsNullOrEmpty(this.FilterTextBox.Text))
            {
                this.FilterTextBox.SelectAll();
            }

            //this.FilterTextBox.SelectionStart = this.FilterTextBox.Text.Length;
            this.ComboListBox.SelectedIndex = 0;

            App.Current.Deactivated += AppDeacivated;
        }

        private async void UpdateCandidate()
        {
            // packageがあったらPacageから取得する
            var lastPackage = this.UsingPackages.LastOrDefault();
            if(lastPackage != null)
            {
                UnitLists.Clear();
                foreach (var unit in lastPackage.GetChildren())
                {
                    UnitLists.Add(unit);
                }
                return;
            }

            //なければ モードから取得する
            string inputText;
            if(this.mode != UrlMode.Instance)
            {
                inputText = this.FilterTextBox.Text.Substring(this.mode.Prefix.Length);
            }
            else
            {
                inputText = this.FilterTextBox.Text;    
            }

            UnitLists.Clear();
            var units = UnitsService.Instnace.CreateUnits(this.Mode, inputText);
            await foreach ( var unit in units )
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

            if (!UsingPackages.Any())
            {
                var resentMode = this.mode;
                var mode = UnitsService.Instnace.GetModeFromText(this.FilterTextBox.Text);
                if (mode == resentMode)
                {
                    if (mode == BookSearchMode.Instance)
                    {
                        UpdateCandidate();
                    }
                }
                else
                {
                    this.Mode = mode;
                    this.UpdateCandidate();
                }
            }

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.ComboListBox.Items);
            view.Filter = this.Filter;
            this.typingTimer = null;
            this.beforeText = this.FilterTextBox.Text;
            this.OnPropertyChanged(nameof(this.UnitLists));
            this.ComboListBox.SelectedIndex = 0;
        }

        private void SnippetToolWindow_OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (this.ComboListBox.SelectedItem is SnippetUnit combo)
                {
                    this.SnippetText = combo.Text;
                    this.CloseWindow();
                }

                ExecuteUnit(e);
            }
            else if(e.Key == Key.Tab)
            {
                ExecuteUnit(e);
            }

            else if (e.Key == Key.Escape)
            {
                this.CloseWindow();
            }
        }

        private void ExecuteUnit(System.Windows.Input.KeyEventArgs e)
        {
            if (this.ComboListBox.SelectedItem is ModeChangeUnit mode)
            {
                // TODO modeの切り替えをショートカットキーでできるようにした際に、Text部分をどうするのか
                var currentText = this.FilterTextBox.Text;
                this.FilterTextBox.Text = mode.Text;
                FilterTextBox.CaretIndex = FilterTextBox.Text.Length;
                e.Handled = true;
                return;
            }

            if (this.ComboListBox.SelectedItem is ICommandUnit command)
            {
                command.Execute();
                e.Handled = true;
                return;
            }

            if (this.ComboListBox.SelectedItem is IUnitPackage package)
            {
                // TODO modeの切り替えをショートカットキーでできるようにした際に、Text部分をどうするのか
                var currentText = this.FilterTextBox.Text;
                this.FilterTextBox.Text = string.Empty;

                UsingPackages.Add(package);
                this.Mode = package.Mode;
                this.PackageText.Text = package.Name;
                this.PackageText.Visibility = Visibility.Visible;
                this.PackgeTextBorder.Visibility = Visibility.Visible;
                UpdateCandidate();

                e.Handled = true;

                return;
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
            else if (e.Key == Key.Back)
            {
                if (string.IsNullOrEmpty(this.FilterTextBox.Text))
                {
                    if (this.UsingPackages.Any())
                    {
                        this.UsingPackages.Remove(this.UsingPackages.Last());
                        var  lastPackage = this.UsingPackages.LastOrDefault();
                        if (lastPackage == null)
                        {
                            this.Mode = StandardMode.Instance;
                            PackgeTextBorder.Visibility = Visibility.Collapsed;
                            this.PackageText.Visibility = Visibility.Collapsed;
                            this.PackageText.Text = string.Empty;
                        }

                        UpdateCandidate();
                    }
                }
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

                if(this.mode == BookSearchMode.Instance)
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

        private void ComboListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ComboListBox.SelectedItem is IUnit combo)
            {
                this.Preview.Children.Clear();

                var uiElement = combo.GetOrCreateUiElemnt();
                this.Preview.Children.Add(uiElement);
            }
        }

        private void AppDeacivated(object? sender, EventArgs e)
        {
#if DEBUG
            return;
#endif
            this.CloseWindow();
        }

        private void CloseWindow()
        {
            if (this.IsClosing)
            {
                return;
            }

            UIElementRepository.RescentText = this.FilterTextBox.Text;
            this.IsClosing = true; ;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGroupDescription();
            if (this.ComboListBox.SelectedItem is IUnit combo)
            {
                this.Preview.Children.Clear();

                var uiElement = combo.GetOrCreateUiElemnt();
                this.Preview.Children.Add(uiElement);
            }

            OnPropertyChanged(nameof(AjustWindowCommand));

            this.Activate();
            this.Focus();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            AppCommands.AimAssistShutdown.Execute();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
        }

        public ICommand AjustWindowCommand { get; set; }

        private GridLength columnWidth = new GridLength(1, GridUnitType.Star);
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            if(e.Source is System.Windows.Controls.ListBox)
            {
                return;
            }

            // GridSplitterを可視化
            if(GridSplitter != null)
            {
                // 前に閉じたときの高さ値が残っていたらそれを復元
                LeftColumn.Width = columnWidth;
                GridSplitter.Visibility = Visibility.Visible;
            }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            if(e.Source is System.Windows.Controls.ListBox)
            {
                return;
            }

            // GridSplitterを非表示に
            if (GridSplitter != null )
            {
                // 閉じる前の高さを保存し
                // 高さをAutoに戻す
                columnWidth = LeftColumn.Width;
                LeftColumn.Width = GridLength.Auto;
                GridSplitter.Visibility = Visibility.Collapsed;
            }
        }
    }
}