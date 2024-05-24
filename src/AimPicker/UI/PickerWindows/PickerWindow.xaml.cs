using AimPicker.Domain;
using AimPicker.Service.Plugins;
using AimPicker.UI.Combos;
using AimPicker.UI.Combos.Commands;
using AimPicker.UI.Combos.Snippets;
using AimPicker.UI.Repositories;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace AimPicker.UI.Tools.Snippets
{
    public partial class PickerWindow : INotifyPropertyChanged
    {
        public Dictionary<IPickerMode, ObservableCollection<IComboViewModel>> ComboDictionary { get; private set; } = new Dictionary<IPickerMode, ObservableCollection<IComboViewModel>>()
        {
            {SnippetMode.Instance, new ObservableCollection<IComboViewModel>()
            {
            new SnippetViewModel("aim","AimNext"),
            new SnippetViewModel("Today",DateTime.Now.ToString("d")),
            new SnippetViewModel("Now",DateTime.Now.ToString("t")),
            new SnippetViewModel("AppData",Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)),
            new SnippetViewModel("Downloads",Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads")),
            new SnippetViewModel("環境変数","control.exe sysdm.cpl,,3"),
            } },
            { WorkFlowMode.Instance ,new ObservableCollection<IComboViewModel>()
            {
            } },
            {CalculationMode.Instance, new ObservableCollection<IComboViewModel>() },
            {UrlMode.Instance, new ObservableCollection<IComboViewModel>(){
            } },
            {BookSearchMode.Instance, new ObservableCollection<IComboViewModel>(){
            } },
        };
        public ObservableCollection<IComboViewModel> ComboLists => this.ComboDictionary[this.Mode];
        public PickerMode Mode { get; set; }
        public string SnippetText { get; set; } = string.Empty;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private bool Filter(object obj)
        {
            var filterText = this.FilterTextBox.Text;
            if (this.Mode != SnippetMode.Instance)
            {
                filterText = filterText.Substring(1);
            }

            if (string.IsNullOrEmpty(filterText))
            {
                return true;
            }

            var combo = obj as SnippetViewModel;
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
            this.FilterTextBox.Focus();
            this.FilterTextBox.SelectionStart = this.FilterTextBox.Text.Length;
            this.ComboListBox.SelectedIndex = 0;

            var pluginService = new PluginsService();
            pluginService.LoadCommandPlugins();
            var combos = pluginService.GetCombos();
            foreach (var item in combos)
            {
                if (item is SnippetViewModel snippet)
                {
                    ComboDictionary[SnippetMode.Instance].Add(snippet);
                }

                if (item is PickerCommandViewModel command)
                {
                    ComboDictionary[WorkFlowMode.Instance].Add(command);
                }
            }

            if (System.Windows.Clipboard.ContainsText())
            {
                ComboDictionary[SnippetMode.Instance].Insert(0, new SnippetViewModel("クリップボード", System.Windows.Clipboard.GetText()));
            }

            ComboDictionary[WorkFlowMode.Instance].Add(new PickerCommandViewModel("ChatGPT", "https://chatgpt.com/", new WebViewPreviewFactory()));


            App.Current.Deactivated += AppDeacivated;
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
                this.Mode = WorkFlowMode.Instance;
            }
            else if (this.FilterTextBox.Text.StartsWith('='))
            {
                this.Mode = CalculationMode.Instance;
            }
            else if (this.FilterTextBox.Text.StartsWith('!'))
            {
                this.Mode = BookSearchMode.Instance;
                this.ComboDictionary[BookSearchMode.Instance].Clear();

                var searchTitleText = this.FilterTextBox.Text.Substring(1);
                InitializeAsync(searchTitleText);



            }
            else if (this.FilterTextBox.Text.StartsWith("https://"))
            {
                this.Mode = UrlMode.Instance;
                var text = this.FilterTextBox.Text;
                this.ComboDictionary[UrlMode.Instance].Clear();
                if (this.FilterTextBox.Text.StartsWith("https://www.amazon"))
                {

                    this.ComboDictionary[UrlMode.Instance].Add(new UrlCommandViewModel("Amazon Preview", text, new AmazonWebViewPreviewFactory()));
                }
                else
                {
                    this.ComboDictionary[UrlMode.Instance].Add(new UrlCommandViewModel("URL Preview", text, new WebViewPreviewFactory()));
                }
            }
            else
            {
                this.Mode = SnippetMode.Instance;
            }

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(this.ComboLists);
            view.Filter = this.Filter;
            this.typingTimer = null;
            this.beforeText = this.FilterTextBox.Text;
            this.OnPropertyChanged(nameof(this.ComboLists));
            this.ComboListBox.SelectedIndex = 0;
        }

        private WebView2 webView;
        private bool iswebloading;
        private Stopwatch timer = new Stopwatch();
        private async void InitializeAsync(string searchText)
        {
            if (iswebloading)
            {
                return;
            }
            timer.Start();
            iswebloading = true;

            webView = new WebView2();
            webView.Height = 0;
            webView.Width = 0;
            this.FilterContents.Children.Clear();
            this.FilterContents.Children.Add(webView);

            await webView.EnsureCoreWebView2Async(null);
            if(webView.CoreWebView2 == null)
            {
                iswebloading = false;
                return;
            }

            webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;

            string apiUrl = $"https://www.googleapis.com/books/v1/volumes?q={searchText}";
            webView.CoreWebView2.Navigate("about:blank"); // Navigate to a blank page to execute JavaScript
            string script = $@"
                fetch('{apiUrl}')
                    .then(response => response.json())
                    .then(data => {{
                        window.chrome.webview.postMessage(data);
                    }})
                    .catch(error => {{
                        console.error('Error:', error);
                    }});
            ";
            await webView.CoreWebView2.ExecuteScriptAsync(script);
            iswebloading = false;
        }

        private void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            webView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string jsonResponse = e.WebMessageAsJson;

            Root bookInfo = JsonConvert.DeserializeObject<Root>(jsonResponse);
            if(bookInfo.items != null)
            {
            foreach(var aa in bookInfo.items)
            {
                var titlte = aa.volumeInfo.title;
                var author = aa.volumeInfo.authors?.FirstOrDefault();
                    if (aa.volumeInfo.industryIdentifiers == null)
                    {
                        continue;
                    }
                    foreach (var bb in aa.volumeInfo.industryIdentifiers)
                {
                    if(bb.type == "ISBN_10")
                    {
                        var url = $"https://www.amazon.co.jp/dp/{bb.identifier}";
                        this.ComboDictionary[BookSearchMode.Instance].Add(new UrlCommandViewModel(titlte , url, new AmazonWebViewPreviewFactory()));
                    }
                }
            }
            }




            timer.Stop();
            File.AppendAllText("C:\\Temp\\test.txt", $"Elapsed Time: {timer.Elapsed.TotalSeconds.ToString()} seconds"+"\r\n");
            timer.Reset();
            iswebloading = false;
        }

        private void SnippetToolWindow_OnKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (this.ComboListBox.SelectedItem is SnippetViewModel combo)
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

                this.typingTimer.Interval = TimeSpan.FromMilliseconds(500);

                this.typingTimer.Tick += this.HandleTypingTimerTimeout;
            }

            this.typingTimer.Stop(); // Resets the timer
            this.typingTimer.Start();
        }

        private void ComboListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.previewWindow == null)
            {
                return;
            }

            if (this.ComboListBox.SelectedItem is IComboViewModel combo)
            {
                this.previewWindow.Contents?.Children.Clear();

                var uiElement = combo.Create();
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

            this.IsClosing = true; ;
            this.previewWindow.Visibility = Visibility.Hidden;
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            if(this.previewWindow == null)
            {
                this.PreviewWindow.Show();
            }
            else
            {
                this.previewWindow.Visibility = Visibility.Visible;
                this.previewWindow.Show();
            }

            AjustWindowCommand = new RelayCommand((o) => { CenterWindowsOnScreen(this, this.previewWindow); });

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