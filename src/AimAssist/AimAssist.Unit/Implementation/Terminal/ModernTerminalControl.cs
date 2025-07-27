using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AimAssist.PtyNet;

namespace AimAssist.Units.Implementation.Terminal
{
    public class ModernTerminalControl : UserControl, IDisposable
    {
        private readonly ScrollViewer _scrollViewer;
        private readonly TextBox _outputTextBox;
        private readonly TextBox _inputTextBox;
        private readonly StringBuilder _outputBuffer;
        private readonly Border _statusBar;
        private readonly TextBlock _statusText;
        private ConPtyTerminal? _terminal;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _disposed = false;

        public ModernTerminalControl()
        {
            InitializeComponent();
            _outputBuffer = new StringBuilder();
            
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // Output area with modern styling
            _outputTextBox = new TextBox
            {
                FontFamily = new FontFamily("Cascadia Code, Consolas, Monaco, monospace"),
                FontSize = 13,
                Background = new SolidColorBrush(Color.FromRgb(12, 12, 12)),
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                Padding = new Thickness(12, 8, 12, 8),
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true,
                BorderThickness = new Thickness(0, 0, 0, 0),
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
                AcceptsReturn = true,
                AcceptsTab = true
            };

            _scrollViewer = new ScrollViewer
            {
                Content = _outputTextBox,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                Background = new SolidColorBrush(Color.FromRgb(12, 12, 12)),
                BorderThickness = new Thickness(0, 0, 0, 0)
            };

            // Input area with modern styling
            _inputTextBox = new TextBox
            {
                FontFamily = new FontFamily("Cascadia Code, Consolas, Monaco, monospace"),
                FontSize = 13,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Foreground = new SolidColorBrush(Color.FromRgb(204, 204, 204)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(63, 63, 70)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(12, 8, 12, 8),
                MinHeight = 32
            };

            // Status bar
            _statusText = new TextBlock
            {
                Text = "準備中...",
                Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153)),
                FontSize = 11,
                Margin = new Thickness(8, 4, 8, 4)
            };

            _statusBar = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(63, 63, 70)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Child = _statusText,
                Height = 24
            };

            _inputTextBox.KeyDown += OnInputKeyDown;

            Grid.SetRow(_scrollViewer, 0);
            Grid.SetRow(_inputTextBox, 1);
            Grid.SetRow(_statusBar, 2);

            mainGrid.Children.Add(_scrollViewer);
            mainGrid.Children.Add(_inputTextBox);
            mainGrid.Children.Add(_statusBar);

            Content = mainGrid;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void InitializeComponent()
        {
            Background = new SolidColorBrush(Color.FromRgb(12, 12, 12));
            Focusable = true;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            await StartTerminalAsync();
            _inputTextBox.Focus();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            StopTerminal();
        }

        private async Task StartTerminalAsync()
        {
            try
            {
                UpdateStatus("ターミナルを初期化中...");

                // Check administrator privileges first
                if (!ConPtyChecker.IsRunningAsAdmin())
                {
                    AppendOutput("==========================================\r\n");
                    AppendOutput("         管理者権限が必要です\r\n");
                    AppendOutput("==========================================\r\n\r\n");
                    AppendOutput("このターミナル機能を使用するには、管理者権限でアプリケーションを実行してください。\r\n\r\n");
                    AppendOutput("手順:\r\n");
                    AppendOutput("1. アプリケーションを終了する\r\n");
                    AppendOutput("2. アプリケーションを右クリック\r\n");
                    AppendOutput("3. \"管理者として実行\" を選択\r\n\r\n");
                    AppendOutput("理由: ConPTY APIの制限により、標準ユーザー権限では\r\n");
                    AppendOutput("ターミナル出力が正常に表示されません。\r\n\r\n");
                    UpdateStatus("管理者権限が必要");
                    return;
                }

                // Check system compatibility
                var systemInfo = ConPtyChecker.GetDetailedDiagnostics();
                AppendOutput($"System Diagnostics:\r\n{systemInfo}\r\n\r\n");

                if (!ConPtyChecker.IsConPtyAvailable())
                {
                    AppendOutput("Error: ConPTY is not available on this system.\r\n");
                    AppendOutput("ConPTY requires Windows 10 version 1809 or later.\r\n");
                    AppendOutput("Please update your Windows version.\r\n");
                    UpdateStatus("ConPTY利用不可");
                    return;
                }

                _terminal = new ConPtyTerminal();
                _terminal.ProcessExited += OnProcessExited;

                var workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var command = "cmd.exe";
                
                AppendOutput($"Starting {command}...\r\n");
                UpdateStatus("ターミナルを開始中...");
                
                var success = await _terminal.StartAsync(command, workingDirectory, 100, 30);
                
                if (success && _terminal.OutputStream != null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    
                    _ = Task.Run(() => ReadOutputAsync(_cancellationTokenSource.Token));
                    
                    AppendOutput("Terminal started successfully. Type 'exit' to close.\r\n");
                    UpdateStatus($"接続済み - {workingDirectory}");
                }
                else
                {
                    AppendOutput("Failed to start terminal.\r\n");
                    UpdateStatus("接続失敗");
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"Failed to start terminal: {ex.Message}\r\n");
                UpdateStatus("エラー");
            }
        }

        private void StopTerminal()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _terminal?.Kill();
                _terminal?.Dispose();
                UpdateStatus("切断済み");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping terminal: {ex.Message}");
            }
        }

        private async Task ReadOutputAsync(CancellationToken cancellationToken)
        {
            if (_terminal?.OutputStream == null) return;

            var buffer = new byte[4096];
            
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var bytesRead = await _terminal.OutputStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    
                    if (bytesRead > 0)
                    {
                        var output = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        await Dispatcher.InvokeAsync(() => AppendOutput(output));
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() => 
                {
                    AppendOutput($"\r\nError reading output: {ex.Message}\r\n");
                    UpdateStatus("読み取りエラー");
                });
            }
        }

        private void AppendOutput(string text)
        {
            _outputBuffer.Append(text);
            _outputTextBox.Text = _outputBuffer.ToString();
            _scrollViewer.ScrollToEnd();
        }

        private void UpdateStatus(string status)
        {
            _statusText.Text = status;
        }

        private async void OnInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                
                var input = _inputTextBox.Text;
                _inputTextBox.Clear();
                
                if (!string.IsNullOrEmpty(input))
                {
                    await SendInputAsync(input + "\r\n");
                }
            }
            else if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+C - send interrupt signal
                await SendInputAsync("\x03");
                e.Handled = true;
            }
            else if (e.Key == Key.L && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                // Ctrl+L - clear screen
                await SendInputAsync("\x0C");
                e.Handled = true;
            }
        }

        private async Task SendInputAsync(string input)
        {
            if (_terminal?.InputStream == null) return;

            try
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                await _terminal.InputStream.WriteAsync(bytes, 0, bytes.Length);
                await _terminal.InputStream.FlushAsync();
            }
            catch (Exception ex)
            {
                AppendOutput($"\r\nError sending input: {ex.Message}\r\n");
                UpdateStatus("入力エラー");
            }
        }

        private void OnProcessExited(object? sender, int exitCode)
        {
            Dispatcher.InvokeAsync(() =>
            {
                AppendOutput($"\r\nProcess exited with code: {exitCode}\r\n");
                _inputTextBox.IsEnabled = false;
                UpdateStatus($"プロセス終了 (コード: {exitCode})");
            });
        }

        public void Reset()
        {
            _outputBuffer.Clear();
            _outputTextBox.Clear();
            _inputTextBox.Clear();
            _inputTextBox.IsEnabled = true;
            UpdateStatus("リセット完了");
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            _inputTextBox?.Focus();
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                StopTerminal();
                _disposed = true;
            }
        }
    }
}