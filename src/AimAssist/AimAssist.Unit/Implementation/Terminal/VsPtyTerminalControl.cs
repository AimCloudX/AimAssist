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
using Pty.Net;

namespace AimAssist.Units.Implementation.Terminal
{
    public class VsPtyTerminalControl : UserControl, IDisposable
    {
        private readonly ScrollViewer _scrollViewer;
        private readonly TextBox _outputTextBox;
        private readonly TextBox _inputTextBox;
        private readonly StringBuilder _outputBuffer;
        private readonly Border _statusBar;
        private readonly TextBlock _statusText;
        private IPtyConnection? _ptyConnection;
        private Process? _fallbackProcess;
        private StreamWriter? _processInput;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _disposed = false;
        private bool _usingFallback = false;
        private bool _isInitialized = false;

        public VsPtyTerminalControl()
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
            if (!_isInitialized && !_disposed)
            {
                await StartTerminalAsync();
                _isInitialized = true;
            }
            _inputTextBox.Focus();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            // UIElementがキャッシュされる場合があるため、Unloaded時にはターミナルを停止しない
            // Disposeメソッドで明示的に停止する
        }

        private async Task StartTerminalAsync()
        {
            try
            {
                UpdateStatus("ターミナルを初期化中...");

                var workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                
                // Fallback to simple process execution if PTY fails
                AppendOutput($"Starting PowerShell terminal using enhanced process execution...\r\n");
                AppendOutput($"Working directory: {workingDirectory}\r\n");
                UpdateStatus("PowerShellターミナルを開始中...");

                // Try PtyProvider first, fallback to process execution
                try
                {
                    var options = new PtyOptions
                    {
                        App = "pwsh.exe",
                        Cwd = workingDirectory,
                        Cols = 100,
                        Rows = 30,
                        CommandLine = new string[] { }
                    };

                    _cancellationTokenSource = new CancellationTokenSource();
                    _ptyConnection = await PtyProvider.SpawnAsync(options, _cancellationTokenSource.Token);
                }
                catch (Exception ptyEx)
                {
                    AppendOutput($"PTY initialization failed: {ptyEx.Message}\r\n");
                    AppendOutput("Falling back to standard process execution...\r\n");
                    
                    await StartFallbackProcessAsync(workingDirectory);
                    return;
                }

                if (_ptyConnection != null)
                {
                    _ptyConnection.ProcessExited += OnProcessExited;
                    
                    _ = Task.Run(() => ReadOutputAsync(_cancellationTokenSource.Token));
                    
                    AppendOutput("PowerShell terminal started successfully using vs-pty.net. Type 'exit' to close.\r\n");
                    UpdateStatus($"PowerShell vs-pty.net接続済み - PID: {_ptyConnection.Pid} - {workingDirectory}");
                }
                else
                {
                    AppendOutput("Failed to start terminal using vs-pty.net.\r\n");
                    UpdateStatus("接続失敗");
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"Failed to start terminal: {ex.Message}\r\n");
                AppendOutput($"Stack trace: {ex.StackTrace}\r\n");
                UpdateStatus("エラー");
            }
        }

        private async Task StartFallbackProcessAsync(string workingDirectory)
        {
            try
            {
                _usingFallback = true;
                
                var processInfo = new ProcessStartInfo
                {
                    FileName = "pwsh.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory,
                    StandardInputEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8
                };

                _fallbackProcess = new Process { StartInfo = processInfo };
                _fallbackProcess.OutputDataReceived += OnFallbackOutputDataReceived;
                _fallbackProcess.ErrorDataReceived += OnFallbackErrorDataReceived;
                _fallbackProcess.Exited += OnFallbackProcessExited;
                _fallbackProcess.EnableRaisingEvents = true;

                if (_fallbackProcess.Start())
                {
                    _processInput = _fallbackProcess.StandardInput;
                    _fallbackProcess.BeginOutputReadLine();
                    _fallbackProcess.BeginErrorReadLine();

                    AppendOutput("PowerShell fallback terminal started successfully.\r\n");
                    UpdateStatus($"PowerShell フォールバック接続済み - PID: {_fallbackProcess.Id} - {workingDirectory}");
                }
                else
                {
                    AppendOutput("Failed to start fallback terminal.\r\n");
                    UpdateStatus("接続失敗");
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"Failed to start fallback terminal: {ex.Message}\r\n");
                UpdateStatus("エラー");
            }
        }

        private void OnFallbackOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Dispatcher.InvokeAsync(() => AppendOutput(e.Data + "\r\n"));
            }
        }

        private void OnFallbackErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Dispatcher.InvokeAsync(() => AppendOutput($"ERROR: {e.Data}\r\n"));
            }
        }

        private void OnFallbackProcessExited(object? sender, EventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                var exitCode = _fallbackProcess?.ExitCode ?? -1;
                AppendOutput($"\r\nProcess exited with code: {exitCode}\r\n");
                _inputTextBox.IsEnabled = false;
                UpdateStatus($"プロセス終了 (コード: {exitCode})");
            });
        }

        private void StopTerminal()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                
                if (_usingFallback)
                {
                    _processInput?.WriteLine("exit");
                    _processInput?.Close();
                    
                    if (_fallbackProcess != null && !_fallbackProcess.HasExited)
                    {
                        _fallbackProcess.WaitForExit(3000);
                        if (!_fallbackProcess.HasExited)
                        {
                            _fallbackProcess.Kill();
                        }
                    }
                    
                    _fallbackProcess?.Dispose();
                }
                else
                {
                    _ptyConnection?.Kill();
                    _ptyConnection?.Dispose();
                }
                
                UpdateStatus("切断済み");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping terminal: {ex.Message}");
            }
        }

        private async Task ReadOutputAsync(CancellationToken cancellationToken)
        {
            if (_ptyConnection?.ReaderStream == null) return;

            var buffer = new byte[4096];
            
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var bytesRead = await _ptyConnection.ReaderStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    
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
                    if (_usingFallback)
                    {
                        AppendOutput($"> {input}\r\n");
                        await SendInputAsync(input);
                    }
                    else
                    {
                        await SendInputAsync(input + "\r\n");
                    }
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
            try
            {
                if (_usingFallback)
                {
                    if (_processInput == null || _fallbackProcess?.HasExited != false) return;
                    
                    await _processInput.WriteLineAsync(input);
                    await _processInput.FlushAsync();
                }
                else
                {
                    if (_ptyConnection?.WriterStream == null) return;
                    
                    var bytes = Encoding.UTF8.GetBytes(input);
                    await _ptyConnection.WriterStream.WriteAsync(bytes, 0, bytes.Length);
                    await _ptyConnection.WriterStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"\r\nError sending input: {ex.Message}\r\n");
                UpdateStatus("入力エラー");
            }
        }

        private void OnProcessExited(object? sender, PtyExitedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                AppendOutput($"\r\nProcess exited with code: {e.ExitCode}\r\n");
                _inputTextBox.IsEnabled = false;
                UpdateStatus($"プロセス終了 (コード: {e.ExitCode})");
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
                _isInitialized = false;
            }
        }
    }
}