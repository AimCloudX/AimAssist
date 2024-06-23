using AimAssist.Combos.Mode.Snippet;
using AimAssist.Core.Commands;
using AimAssist.Service;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Knowledge;
using AimAssist.Unit.Implementation.Options;
using AimAssist.Unit.Implementation.Snippets;
using AimAssist.Unit.Implementation.Standard;
using AimAssist.Unit.Implementation.Web.Bookmarks;
using AimAssist.Unit.Implementation.Web.BookSearch;
using AimAssist.Unit.Implementation.Web.Rss;
using AimAssist.Unit.Implementation.Web.Urls;
using AimAssist.Unit.Implementation.WorkTools;
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
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<IUnit> UnitLists { get; } = new ObservableCollection<IUnit>();
        private IPickerMode mode = StandardMode.Instance;
        private readonly KeySequenceManager _keySequenceManager = new KeySequenceManager();

        public MainWindow()
        {
            InitializeComponent();
            LoadIcons();
            this.DataContext = this;
            this.UpdateCandidate();
            this.ComboListBox.SelectedIndex = 0;
            PreviewKeyDown += MainWindow_PreviewKeyDown;

            var aa = new CommandBinding();
            CommandBinding binding = new CommandBinding(AimAssistCommands.SendUnitCommand, ExecuteReceiveData);
            CommandManager.RegisterClassCommandBinding(typeof(Window), binding);

        }
        private void ExecuteReceiveData(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is UnitsArgs unitsArgs)
            {
                if (unitsArgs.NeedSetMode)
                {
                    if(this.mode != unitsArgs.Mode)
                    {
                        this.mode = unitsArgs.Mode;
                    }
                }

                UpdateCandidate();
                UpdateGroupDescription();
                this.ComboListBox.SelectedIndex = 0;

                foreach (var unit in unitsArgs.Units)
                {
                    UnitLists.Add(unit);
                }
            }
        }

        private void LoadIcons()
        {
            var icons = new List<IPickerMode>
        {
                StandardMode.Instance,
                EditorMode.Instance,
                WorkToolsMode.Instance,
                BookSearchMode.Instance,
                BookmarkMode.Instance,
                OptionMode.Instance,
                CalculationMode.Instance,
                RssMode.Instance,
                SnippetMode.Instance,
                KnowledgeMode.Instance,
        };

            ModeList.ItemsSource = icons;
        }

        private async void IconListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UnitLists.Clear();
           if (ModeList.SelectedItem is IPickerMode mode)
            {
                this.mode = mode;
                var units = UnitsService.Instnace.CreateUnits(mode, string.Empty);
                await foreach (var unit in units)
                {
                    UnitLists.Add(unit);
                }

                UpdateGroupDescription();
                this.ComboListBox.SelectedIndex = 0;
            }
        }

        private void ComboListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ComboListBox.SelectedItem is IUnit combo)
            {
                var uiElement = combo.GetOrCreateUiElemnt();
                this.MainContent.Content = uiElement;
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
                    groupedItems.GroupDescriptions.Add(new PropertyGroupDescription("Mode.Name"));
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

            if (string.IsNullOrEmpty(filterText))
            {
                return true;
            }
            if (this.mode == UrlMode.Instance || this.mode == BookSearchMode.Instance)
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

        private async void UpdateCandidate()
        {
            string inputText;
            inputText = this.FilterTextBox.Text;

            UnitLists.Clear();
            var units = UnitsService.Instnace.CreateUnits(this.mode, inputText);
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
            else if (e.Key == Key.Tab)
            {
                ExecuteUnit(e);
            }
        }

        private void ExecuteUnit(System.Windows.Input.KeyEventArgs e)
        {
            if (this.ComboListBox.SelectedItem is ICommandUnit command)
            {
                command.Execute();
                e.Handled = true;
                return;
            }

            //if (this.ComboListBox.SelectedItem is IUnitPackage package)
            //{
            //    // TODO modeの切り替えをショートカットキーでできるようにした際に、Text部分をどうするのか
            //    var currentText = this.FilterTextBox.Text;
            //    this.FilterTextBox.Text = string.Empty;

            //    UsingPackages.Add(package);
            //    this.Mode = package.Mode;
            //    this.PackageText.Text = package.Name;
            //    this.PackageText.Visibility = Visibility.Visible;
            //    this.PackgeTextBorder.Visibility = Visibility.Visible;
            //    UpdateCandidate();

            //    e.Handled = true;

            //    return;
            //}
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

                if (this.mode == BookSearchMode.Instance)
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

            UIElementRepository.RescentText = this.FilterTextBox.Text;
            this.IsClosing = true; ;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGroupDescription();

            this.Activate();
            this.Focus();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            AppCommands.ShutdownAimAssist.Execute();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(_keySequenceManager.HandleKeyPress(e.Key, Keyboard.Modifiers))
            {
                e.Handled = true;
            }
        }
    }
}