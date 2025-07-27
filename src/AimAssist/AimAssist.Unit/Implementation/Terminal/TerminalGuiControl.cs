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
using System.Windows.Threading;

namespace AimAssist.Units.Implementation.Terminal
{
    public class TerminalGuiControl : UserControl, IDisposable
    {
        private readonly ScrollViewer _scrollViewer;
        private readonly TextBox _outputTextBox;
        private readonly TextBox _inputTextBox;
        private readonly StringBuilder _outputBuffer;
        private readonly Border _statusBar;
        private readonly TextBlock _statusText;
        private Process? _process;
        private StreamWriter? _processInput;
        private bool _disposed = false;

        public TerminalGuiControl()
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

                var workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                
                AppendOutput($"Starting enhanced terminal...\r\n");
                AppendOutput($"Working directory: {workingDirectory}\r\n");
                AppendOutput($"Type 'help' for available commands or 'exit' to close.\r\n\r\n");
                UpdateStatus("ターミナルを開始中...");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory,
                    StandardInputEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8
                };

                _process = new Process { StartInfo = processInfo };
                _process.OutputDataReceived += OnOutputDataReceived;
                _process.ErrorDataReceived += OnErrorDataReceived;
                _process.Exited += OnProcessExited;
                _process.EnableRaisingEvents = true;

                if (_process.Start())
                {
                    _processInput = _process.StandardInput;
                    _process.BeginOutputReadLine();
                    _process.BeginErrorReadLine();

                    AppendOutput("Enhanced terminal started successfully.\r\n");
                    UpdateStatus($"接続済み - PID: {_process.Id} - {workingDirectory}");
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
                _processInput?.WriteLine("exit");
                _processInput?.Close();
                
                if (_process != null && !_process.HasExited)
                {
                    _process.WaitForExit(3000);
                    if (!_process.HasExited)
                    {
                        _process.Kill();
                    }
                }
                
                _process?.Dispose();
                UpdateStatus("切断済み");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error stopping terminal: {ex.Message}");
            }
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Dispatcher.InvokeAsync(() => AppendOutput(e.Data + "\r\n"));
            }
        }

        private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Dispatcher.InvokeAsync(() => AppendOutput($"ERROR: {e.Data}\r\n"));
            }
        }

        private void OnProcessExited(object? sender, EventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                var exitCode = _process?.ExitCode ?? -1;
                AppendOutput($"\r\nProcess exited with code: {exitCode}\r\n");
                _inputTextBox.IsEnabled = false;
                UpdateStatus($"プロセス終了 (コード: {exitCode})");
            });
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
                    AppendOutput($"> {input}\r\n");
                    await SendInputAsync(input);
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
                ClearOutput();
                e.Handled = true;
            }
        }

        private async Task SendInputAsync(string input)
        {
            if (_processInput == null || _process?.HasExited != false) return;

            try
            {
                await _processInput.WriteLineAsync(input);
                await _processInput.FlushAsync();
            }
            catch (Exception ex)
            {
                AppendOutput($"\r\nError sending input: {ex.Message}\r\n");
                UpdateStatus("入力エラー");
            }
        }

        private void ClearOutput()
        {
            _outputBuffer.Clear();
            _outputTextBox.Clear();
        }

        public void Reset()
        {
            ClearOutput();
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