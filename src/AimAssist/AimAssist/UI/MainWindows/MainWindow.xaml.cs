using AimAssist.Core.Commands;
using AimAssist.Service;
using AimAssist.UI.PickerWindows;
using AimAssist.UI.UnitContentsView;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Modes;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.KeyHelp;
using AimAssist.Units.Implementation.Snippets;
using AimAssist.Units.Implementation.Web.BookSearch;
using Common.Commands;
using Common.Commands.Shortcus;
using Common.UI;
using Common.UI.WebUI;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AimAssist.UI.MainWindows
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<UnitViewModel> UnitLists { get; } = new ObservableCollection<UnitViewModel>();
        public ObservableCollection<UnitViewModel> ActiveUnits { get; } = new ObservableCollection<UnitViewModel>();
        private IMode mode;
        private readonly KeySequenceManager _keySequenceManager = new KeySequenceManager();

 private void MainWindow_SourceInitialized(object sender, EventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;
        HwndSource.FromHwnd(handle)?.AddHook(new HwndSourceHook(WndProc));
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_SYSKEYDOWN = 0x0104;
        const int VK_MENU = 0x12; // Alt key

        if (msg == WM_SYSKEYDOWN && wParam.ToInt32() == VK_MENU)
        {
            handled = true;
            return IntPtr.Zero;
        }

        return IntPtr.Zero;
    }
        public MainWindow()
        {
            InitializeComponent();
            SourceInitialized += MainWindow_SourceInitialized;

            RegisterCommands();

            LoadIcons();
            this.DataContext = this;
            this.ModeList.SelectedItem = AllInclusiveMode.Instance;
            this.UpdateCandidate();
            this.ComboListBox.SelectedIndex = 0;
            KeyDown += MainWindow_PreviewKeyDown;
            this.FilterTextBox.Focus();

            var binding = new CommandBinding(AimAssistCommands.SendUnitCommand, ExecuteReceiveData);
            CommandManager.RegisterClassCommandBinding(typeof(Window), binding);

        }

        private async void RegisterCommands()
        {
            foreach (var mode in UnitsService.Instnace.GetAllModes())
            {
                var modeChangeCommand = new RelayCommand(mode.GetModeChnageCommandName(), (window) =>
                {
                    if (window is MainWindow mainWindow)
                    {
                        mainWindow.ModeList.SelectedItem = mode;
                        mainWindow.FilterTextBox.Focus();
                    }
                }
 );
                CommandService.Register(modeChangeCommand, mode.DefaultKeySequence);
            }

            MainWindowCommands.NextMode = new RelayCommand(nameof(MainWindowCommands.NextMode), (Window window) =>
            {
                if (window is MainWindow mainWindow)
                {
                    var index = mainWindow.ModeList.SelectedIndex;
                    if (index == mainWindow.ModeList.Items.Count - 1)
                    {
                        mainWindow.ModeList.SelectedIndex = 0;
                    }
                    else
                    {
                        mainWindow.ModeList.SelectedIndex = index + 1;
                    }
                }
            });

            MainWindowCommands.PreviousMode = new RelayCommand(nameof(MainWindowCommands.PreviousMode), (Window window) =>
            {
                if (window is MainWindow mainWindow)
                {
                    var index = mainWindow.ModeList.SelectedIndex;
                    if (index == 0)
                    {
                        mainWindow.ModeList.SelectedIndex = mainWindow.ModeList.Items.Count - 1;
                    }
                    else
                    {
                        mainWindow.ModeList.SelectedIndex = index - 1;
                    }
                }
            });

            MainWindowCommands.NextUnit = new RelayCommand(nameof(MainWindowCommands.NextUnit), (Window window) =>
            {
                if (window is MainWindow mainWindow)
                {
                    if (this.ModeList.Items.Count == 0)
                    {
                        return;
                    }

                    var index = this.ComboListBox.SelectedIndex;
                    if (index == this.ComboListBox.Items.Count - 1)
                    {
                        this.ComboListBox.SelectedIndex = 0;
                    }
                    else
                    {
                        this.ComboListBox.SelectedIndex = index + 1;
                    }
                }
            });
            MainWindowCommands.PreviousUnit = new RelayCommand(nameof(MainWindowCommands.PreviousUnit), (Window window) =>
            {
                if (window is MainWindow mainWindow)
                {
                    if (mainWindow.ModeList.Items.Count == 0)
                    {
                        return;
                    }

                    var index = mainWindow.ComboListBox.SelectedIndex;
                    if (index == 0)
                    {
                        mainWindow.ComboListBox.SelectedIndex = mainWindow.ComboListBox.Items.Count - 1;
                    }
                    else
                    {
                        mainWindow.ComboListBox.SelectedIndex = index - 1;
                    }
                }
            });

            MainWindowCommands.FocusContent = new RelayCommand(nameof(MainWindowCommands.FocusContent), (Window window) =>
            {
                if (window is MainWindow mainWindow)
                {
                    mainWindow.FocusContent();
                }

                if (window is PickerWindow pickerWindow)
                {
                    pickerWindow.FocusContent();
                }
            });

            MainWindowCommands.FocusUnits = new RelayCommand(nameof(MainWindowCommands.FocusUnits), (Window window)  =>
            {
                if (window is MainWindow mainWindow)
                {
                    mainWindow.FilterTextBox.Focus();
                }

                if (window is PickerWindow pickerWindow)
                {
                    pickerWindow.FilterTextBox.Focus();
                }
            });

            MainWindowCommands.RemoveActiveView = new RelayCommand(nameof(MainWindowCommands.RemoveActiveView), (Window window) =>
            {
                if (window is MainWindow mainWindow)
                {
                    if (mainWindow.mode == ActiveUnitMode.Instance)
                    {
                        if (mainWindow.ComboListBox.SelectedItem is UnitViewModel unit)
                        {
                            mainWindow.ActiveUnits.Remove(unit);
                            mainWindow.UnitLists.Remove(unit);
                        }
                    }
                }
            });

            CommandService.Register(MainWindowCommands.NextMode, new KeySequence(Key.N, ModifierKeys.Control | ModifierKeys.Shift));
            CommandService.Register(MainWindowCommands.PreviousMode, new KeySequence(Key.P, ModifierKeys.Control | ModifierKeys.Shift));
            CommandService.Register(MainWindowCommands.NextUnit, new KeySequence(Key.N, ModifierKeys.Control));
            CommandService.Register(MainWindowCommands.PreviousUnit, new KeySequence(Key.P, ModifierKeys.Control));

            CommandService.Register(MainWindowCommands.FocusContent, new KeySequence(Key.K, ModifierKeys.Control, Key.L, ModifierKeys.Control));
            CommandService.Register(MainWindowCommands.FocusUnits, new KeySequence(Key.K, ModifierKeys.Control, Key.J, ModifierKeys.Control));
            CommandService.Register(MainWindowCommands.RemoveActiveView, new KeySequence(Key.W, ModifierKeys.Control));

            foreach (var unit in UnitsService.Instnace.CreateUnits(AllInclusiveMode.Instance))
            {
                var unitChangeCommand = new RelayCommand(unit.Name, (Window window) =>
                {
                    if (window is MainWindow mainWindow)
                    {
                        mainWindow.ModeList.SelectedItem = unit.Mode;
                        mainWindow.ComboListBox.SelectedItem = UnitLists.FirstOrDefault(x => x.Name == unit.Name);
                    }
                });

                CommandService.Register(unitChangeCommand, KeySequence.None);
            }

        }

        private void ExecuteReceiveData(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter is UnitsArgs unitsArgs)
            {
                if (unitsArgs.NeedSetMode)
                {
                    if (this.mode != unitsArgs.Mode)
                    {
                        this.mode = unitsArgs.Mode;
                    }
                }

                UpdateCandidate();
                UpdateGroupDescription();
                this.ComboListBox.SelectedIndex = 0;


                Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                foreach(var unit in unitsArgs.Units)
                {
                    if(unit is UrlUnit urlUnit)
                    {
                        UnitLists.Add(new UnitViewModel(urlUnit, this));
                    }
                    else
                    {
                        UnitLists.Add(new UnitViewModel(unit));
                    }
                }

                Mouse.OverrideCursor = null;
            }
        }

        private void LoadIcons()
        {
            ModeList.ItemsSource = UnitsService.Instnace.GetAllModes();
        }

        private async void IconListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UnitLists.Clear();
            if (ModeList.SelectedItem is IMode mode)
            {
                this.mode = mode;
                UpdateCandidate();

                UpdateGroupDescription();
                this.ComboListBox.SelectedIndex = 0;
            }
        }

        private void ComboListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ComboListBox.SelectedItem is UnitViewModel unit)
            {
                var view = new UnitViewFactory().Create(unit);
                this.MainContent.Content = view;
            }
        }

        private void UpdateGroupDescription()
        {
            CollectionViewSource groupedItems = (CollectionViewSource)this.Resources["GroupedItems"];

            if (groupedItems != null)
            {
                groupedItems.GroupDescriptions.Clear();
                if (this.mode == AllInclusiveMode.Instance)
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

        protected void OnPropertyChanged([CallerMemberName] string name = "")
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

            var combo = obj as UnitViewModel;
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
            if (this.mode == ActiveUnitMode.Instance)
            {
                foreach (var unit in ActiveUnits)
                {
                    UnitLists.Add(unit);
                }

                return;
            }

            var units = UnitsService.Instnace.CreateUnits(this.mode);

            foreach (var unit in units)
            {
                if (unit is UrlUnit urlUnit)
                {
                    UnitLists.Add(new UnitViewModel(urlUnit, this));
                }
                else if (unit is SnippetModelUnit)
                {
                    if (this.mode == SnippetMode.Instance)
                    {
                        UnitLists.Add(new UnitViewModel(unit));
                    }
                }
                else
                {
                    UnitLists.Add(new UnitViewModel(unit));
                }
            }
        }

        public static BitmapImage GetUrlIcon(UrlUnit urlUnit)
        {
            string url = urlUnit.Description;
            try
            {
                Uri uri = new Uri(url);
                //string faviconUrl = $"{uri.Scheme}://{uri.Host}/favicon.ico";
                string faviconUrl = GetFaviconUrl(urlUnit.Description);

                using (WebClient client = new WebClient())
                {
                    // User-Agentヘッダーを追加
                    client.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                    byte[] data = client.DownloadData(faviconUrl);

                    using (MemoryStream ms = new MemoryStream(data))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.StreamSource = ms;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        bitmap.Freeze();

                        Console.WriteLine("ChatGPTのアイコンを正常に取得しました。");
                        return bitmap;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChatGPTのアイコンの取得に失敗しました: {ex.Message}");
                return null;
            }
        }
        public static string GetFaviconUrl(string domain)
        {
            return $"http://www.google.com/s2/favicons?domain={domain}";
        }

        private void HandleTypingTimerTimeout(object sender, EventArgs e)
        {
            if (this.typingTimer != null)
            {
                this.typingTimer.Tick -= HandleTypingTimerTimeout;
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
                if (this.mode != ActiveUnitMode.Instance)
                {

                    if (this.ComboListBox.SelectedItem is UnitViewModel unit)
                    {
                        if (!ActiveUnits.Contains(unit))
                        {
                            ActiveUnits.Add(unit);
                        }

                        CommandService.Execute(ActiveUnitMode.Instance.GetModeChnageCommandName(), this);
                        this.ComboListBox.SelectedItem = unit;
                    }
                }
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateGroupDescription();

            this.Activate();
            this.Focus();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            AppCommands.ShutdownAimAssist.Execute(this);
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
        }

        private void MainWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (_keySequenceManager.HandleKeyPress(e.Key, Keyboard.Modifiers, this))
            {
                e.Handled = true;
            }
        }

        public void FocusContent()
        {
            if (this.MainContent.Content is IFocasable focusable)
            {
                focusable.Focus();
            }
        }

        private void FilterTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.FilterTextBox.Text = string.Empty;
            }
        }
    }
}