using AimAssist.Core.Interfaces;
using AimAssist.Service;
using AimAssist.Commands;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Common.UI.Commands.Shortcus;
using Microsoft.Extensions.DependencyInjection;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using RelayCommand = Common.UI.Commands.RelayCommand;

namespace AimAssist.UI.MainWindows
{
    public partial class MainWindow : Window, IMainWindow
    {
        private readonly MainWindowViewModel viewModel;
        private readonly IApplicationLogService logService;
        private readonly KeySequenceManager keySequenceManager;
        private readonly IUnitsService unitsService;
        private readonly ICommandService commandService;

        public MainWindow(
            MainWindowViewModel viewModel,
            IApplicationLogService logService,
            KeySequenceManager keySequenceManager,
            IUnitsService unitsService,
            ICommandService commandService)
        {
            this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            this.logService = logService ?? throw new ArgumentNullException(nameof(logService));
            this.keySequenceManager = keySequenceManager ?? throw new ArgumentNullException(nameof(keySequenceManager));
            this.unitsService = unitsService ?? throw new ArgumentNullException(nameof(unitsService));
            this.commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            
            InitializeComponent();
            
            DataContext = this.viewModel;
            
            SourceInitialized += MainWindow_SourceInitialized;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            KeyDown += MainWindow_KeyDown;
            
            RegisterCommands();
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (keySequenceManager.HandleKeyPress(e.Key, Keyboard.Modifiers, this))
            {
                e.Handled = true;
            }
        }

        private void MainWindow_SourceInitialized(object sender, EventArgs e)
        {
            try
            {
                var handle = new WindowInteropHelper(this).Handle;
                HwndSource.FromHwnd(handle)?.AddHook(WndProc);
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "ウィンドウ初期化中にエラーが発生しました");
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_SYSKEYDOWN = 0x0104;
            const int VK_MENU = 0x12;

            if (msg == WM_SYSKEYDOWN && wParam.ToInt32() == VK_MENU)
            {
                handled = true;
                return IntPtr.Zero;
            }

            return IntPtr.Zero;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Activate();
                Focus();
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "ウィンドウ読み込み中にエラーが発生しました");
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (commandService.TryGetCommand(nameof(IAppCommands.ShutdownAimAssist), out var command))
                {
                    command.Execute(this);
                }
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "ウィンドウ終了処理中にエラーが発生しました");
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                viewModel.IsItemListVisible = !viewModel.IsItemListVisible;
                
                if (viewModel.IsItemListVisible)
                {
                    ItemListColumn.Width = new GridLength(220);
                }
                else
                {
                    ItemListColumn.Width = new GridLength(0);
                }
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "ハンバーガーメニュー操作中にエラーが発生しました");
            }
        }

        private void RegisterCommands()
        {
            try
            {
                // モード変更コマンドの登録
                foreach (var mode in unitsService.GetAllModes())
                {
                    var modeChangeCommand = new RelayCommand(mode.GetModeChnageCommandName(), (window) =>
                    {
                        if (window is MainWindow mainWindow)
                        {
                            mainWindow.viewModel.SelectedMode = mode;
                        }
                    });
                    commandService.Register(modeChangeCommand, mode.DefaultKeySequence);
                }

                // メインウィンドウ用コマンドの登録
                MainWindowCommands.NextMode = new RelayCommand(nameof(MainWindowCommands.NextMode), (Window window) =>
                {
                    if (window is MainWindow mainWindow)
                    {
                        var modes = mainWindow.viewModel.Modes;
                        var currentIndex = modes.IndexOf(mainWindow.viewModel.SelectedMode);
                        if (currentIndex == modes.Count - 1)
                        {
                            mainWindow.viewModel.SelectedMode = modes[0];
                        }
                        else
                        {
                            mainWindow.viewModel.SelectedMode = modes[currentIndex + 1];
                        }
                    }
                });

                MainWindowCommands.PreviousMode = new RelayCommand(nameof(MainWindowCommands.PreviousMode), (Window window) =>
                {
                    if (window is MainWindow mainWindow)
                    {
                        var modes = mainWindow.viewModel.Modes;
                        var currentIndex = modes.IndexOf(mainWindow.viewModel.SelectedMode);
                        if (currentIndex == 0)
                        {
                            mainWindow.viewModel.SelectedMode = modes[modes.Count - 1];
                        }
                        else
                        {
                            mainWindow.viewModel.SelectedMode = modes[currentIndex - 1];
                        }
                    }
                });

                MainWindowCommands.NextUnit = new RelayCommand(nameof(MainWindowCommands.NextUnit), (Window window) =>
                {
                    if (window is MainWindow mainWindow)
                    {
                        var units = mainWindow.viewModel.Units;
                        var currentIndex = units.IndexOf(mainWindow.viewModel.SelectedUnit);
                        if (currentIndex == units.Count - 1)
                        {
                            mainWindow.viewModel.SelectedUnit = units[0];
                        }
                        else
                        {
                            mainWindow.viewModel.SelectedUnit = units[currentIndex + 1];
                        }
                    }
                });

                MainWindowCommands.PreviousUnit = new RelayCommand(nameof(MainWindowCommands.PreviousUnit), (Window window) =>
                {
                    if (window is MainWindow mainWindow)
                    {
                        var units = mainWindow.viewModel.Units;
                        var currentIndex = units.IndexOf(mainWindow.viewModel.SelectedUnit);
                        if (currentIndex == 0)
                        {
                            mainWindow.viewModel.SelectedUnit = units[units.Count - 1];
                        }
                        else
                        {
                            mainWindow.viewModel.SelectedUnit = units[currentIndex - 1];
                        }
                    }
                });

                MainWindowCommands.FocusContent = new RelayCommand(nameof(MainWindowCommands.FocusContent), (Window window) =>
                {
                    if (window is MainWindow mainWindow)
                    {
                        mainWindow.FocusContent();
                    }
                });

                MainWindowCommands.FocusUnits = new RelayCommand(nameof(MainWindowCommands.FocusUnits), (Window window) =>
                {
                    if (window is MainWindow mainWindow)
                    {
                        mainWindow.FocusFilterTextBox();
                    }
                });

                // キーバインディングの登録
                commandService.Register(MainWindowCommands.NextMode, new KeySequence(Key.N, ModifierKeys.Control | ModifierKeys.Shift));
                commandService.Register(MainWindowCommands.PreviousMode, new KeySequence(Key.P, ModifierKeys.Control | ModifierKeys.Shift));
                commandService.Register(MainWindowCommands.NextUnit, new KeySequence(Key.N, ModifierKeys.Control));
                commandService.Register(MainWindowCommands.PreviousUnit, new KeySequence(Key.P, ModifierKeys.Control));
                commandService.Register(MainWindowCommands.FocusContent, new KeySequence(Key.K, ModifierKeys.Control, Key.L, ModifierKeys.Control));
                commandService.Register(MainWindowCommands.FocusUnits, new KeySequence(Key.K, ModifierKeys.Control, Key.J, ModifierKeys.Control));
            }
            catch (Exception ex)
            {
                logService.LogException(ex, "コマンド登録中にエラーが発生しました");
            }
        }

        public void FocusContent()
        {
            if (viewModel.CurrentContent is Common.UI.IFocasable focusable)
            {
                focusable.Focus();
            }
        }

        public void FocusFilterTextBox()
        {
            FilterTextBox?.Focus();
        }
    }
}
