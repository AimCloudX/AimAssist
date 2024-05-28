using AimPicker.Combos.Mode.Snippet;
using AimPicker.Service;
using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Standard;
using AimPicker.Unit.Implementation.Web.BookSearch;
using AimPicker.Unit.Implementation.Web.Urls;
using AimPicker.WebViewCash;
using AimPicker.Windows.PickerWindows;
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
    public partial class PickerWindow : INotifyPropertyChanged
    {
        public ObservableCollection<IUnit> UnitLists { get; } = new ObservableCollection<IUnit>();
        private IPickerMode mode;
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
                UpdateCandidate();
                // CollectionViewSourceの取得
                CollectionViewSource groupedItems = (CollectionViewSource)this.Resources["GroupedItems"];

                if (groupedItems != null)
                {
                    groupedItems.GroupDescriptions.Clear();
                    if(this.mode == StandardMode.Instance)
                    {
                        groupedItems.GroupDescriptions.Add(new CustomGroupDescription());
                    }
                    else
                    {
                        groupedItems.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
                    }
                }
                OnPropertyChanged();
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
            filterText = filterText.Substring(this.mode.Prefix.Length);

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

    public partial class PickerWindow : Window
    {
        public bool IsClosing { get; set; }

        DispatcherTimer? typingTimer;
        private string beforeText = string.Empty;
        private PreviewWindow previewWindow;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PickerWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;
            
            this.Mode = StandardMode.Instance;
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
                if(this.ComboListBox.SelectedItem is ModeChangeUnit mode)
                {
                    var currentText = this.FilterTextBox.Text;
                    this.FilterTextBox.Text = mode.Text;
                    FilterTextBox.CaretIndex = FilterTextBox.Text.Length;
                    return;
                }

                if (this.ComboListBox.SelectedItem is SnippetUnit combo)
                {
                    this.SnippetText = combo.Text;
                }

                this.CloseWindow();
            }
            else if(e.Key == Key.Tab)
            {
                if(this.ComboListBox.SelectedItem is ModeChangeUnit mode)
                {
                    // TODO modeの切り替えをショートカットキーでできるようにした際に、Text部分をどうするのか
                    var currentText = this.FilterTextBox.Text;
                    this.FilterTextBox.Text = mode.Text;
                    FilterTextBox.CaretIndex = FilterTextBox.Text.Length;
                    e.Handled = true;
                    return;
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
            if(this.previewWindow == null)
            {
                return;
            }

            if (this.ComboListBox.SelectedItem is IUnit combo)
            {
                this.PreviewWindow.Contents.Children.Clear();

                var uiElement = combo.GetOrCreateUiElemnt();
                this.previewWindow?.Contents?.Children.Add(uiElement);
            }
        }

        public PreviewWindow PreviewWindow 
        { 
            get
            {
                if(this.previewWindow == null)
                {
                    var previewWindow = UIElementRepository.PreviewWindow;
                    if(previewWindow != null) 
                    {
                        this.previewWindow = previewWindow;
                    }
                    else
                    {
                        previewWindow = new PreviewWindow();
                        previewWindow.Topmost = true;

                        // MainWindowの位置とサイズを取得
                        double mainWindowLeft = this.Left;
                        double mainWindowTop = this.Top;
                        double mainWindowWidth = this.Width;
                        double mainWindowHeight = this.Height;

                        // SecondaryWindowの位置を設定
                        previewWindow.Left = mainWindowLeft + mainWindowWidth;
                        previewWindow.Top = mainWindowTop + mainWindowHeight + 130;
                        //secondaryWindow.Top = mainWindowTop;

                        UIElementRepository.PreviewWindow = previewWindow;
                        this.previewWindow = previewWindow;
                    }
                }

                return this.previewWindow;
            } }

        private void AppDeacivated(object? sender, EventArgs e)
        {
            //this.CloseWindow();
        }

        private void CloseWindow()
        {
            if (this.IsClosing)
            {
                return;
            }

            UIElementRepository.RescentText = this.FilterTextBox.Text;
            this.IsClosing = true; ;
            this.previewWindow.Visibility = Visibility.Hidden;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.PreviewWindow.Show();
            if (this.ComboListBox.SelectedItem is IUnit combo)
            {
                this.PreviewWindow.Contents.Children.Clear();

                var uiElement = combo.GetOrCreateUiElemnt();
                this.previewWindow?.Contents?.Children.Add(uiElement);
            }

            AjustWindowCommand = new RelayCommand((o) => { CenterWindowsOnScreen(this, this.previewWindow); });
            OnPropertyChanged(nameof(AjustWindowCommand));

            this.Topmost = true;
            this.Activate();
            this.Focus();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            this.IsClosing = true;
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            InitializeCenterWindowsOnScreen(this, this.previewWindow);
        }

        public ICommand AjustWindowCommand { get; set; }

        private void CenterWindowsOnScreen(Window mainWindow, Window secondaryWindow)
        {
            // Get the screen width and height
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            // Get the width and height of both windows
            double mainWindowWidth = mainWindow.ActualWidth;
            double mainWindowHeight = mainWindow.ActualHeight;
            double secondaryWindowWidth = secondaryWindow.ActualWidth;
            double secondaryWindowHeight = secondaryWindow.ActualHeight;

            // Calculate the total width of both windows
            double totalWidth = mainWindowWidth + secondaryWindowWidth;

            // Calculate the left position for each window
            double mainWindowLeft = (screenWidth - totalWidth) / 2;
            double secondaryWindowLeft = mainWindowLeft + mainWindowWidth;

            // Calculate the top position for both windows
            double topPosition = (screenHeight - Math.Max(mainWindowHeight, secondaryWindowHeight)) / 2;

            // Set the position of the main window
            mainWindow.Left = mainWindowLeft;
            mainWindow.Top = topPosition;

            // Set the position of the secondary window
            secondaryWindow.Left = secondaryWindowLeft;
            secondaryWindow.Top = topPosition;
        }
        private void InitializeCenterWindowsOnScreen(Window mainWindow, Window secondaryWindow)
        {
            // Get the screen width and height
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            // Get the width and height of both windows
            double mainWindowWidth = mainWindow.ActualWidth;
            double mainWindowHeight = mainWindow.ActualHeight;
            double secondaryWindowWidth = 400;
            double secondaryWindowHeight = 500;

            // Calculate the total width of both windows
            double totalWidth = mainWindowWidth + secondaryWindowWidth;

            // Calculate the left position for each window
            double mainWindowLeft = (screenWidth - totalWidth) / 2;
            double secondaryWindowLeft = mainWindowLeft + mainWindowWidth;

            // Calculate the top position for both windows
            double topPosition = (screenHeight - Math.Max(mainWindowHeight, secondaryWindowHeight)) / 2;

            // Set the position of the main window
            mainWindow.Left = mainWindowLeft;
            mainWindow.Top = topPosition;

            // Set the position of the secondary window
            secondaryWindow.Left = secondaryWindowLeft;
            secondaryWindow.Top = topPosition;
        }
    }
}