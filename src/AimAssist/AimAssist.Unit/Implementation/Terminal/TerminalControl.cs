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
    public class TerminalControl : UserControl
    {
        private readonly ScrollViewer _scrollViewer;
        private readonly TextBox _outputTextBox;
        private readonly TextBox _inputTextBox;
        private readonly StringBuilder _outputBuffer;
        private ConPtyTerminal? _terminal;
        private CancellationTokenSource? _cancellationTokenSource;

        public TerminalControl()
        {
            InitializeComponent();
            _outputBuffer = new StringBuilder();
            
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            _outputTextBox = new TextBox
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Background = Brushes.Black,
                Foreground = Brushes.White,
                Padding = new Thickness(5),
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true,
                BorderThickness = new Thickness(0),
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden
            };

            _scrollViewer = new ScrollViewer
            {
                Content = _outputTextBox,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            _inputTextBox = new TextBox
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Background = Brushes.Black,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Padding = new Thickness(5, 2, 5, 2)
            };

            _inputTextBox.KeyDown += OnInputKeyDown;

            Grid.SetRow(_scrollViewer, 0);
            Grid.SetRow(_inputTextBox, 1);

            grid.Children.Add(_scrollViewer);
            grid.Children.Add(_inputTextBox);

            Content = grid;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void InitializeComponent()
        {
            Background = Brushes.Black;
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
                    return;
                }

                _terminal = new ConPtyTerminal();
                _terminal.ProcessExited += OnProcessExited;

                var workingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var command = "cmd.exe";
                
                AppendOutput($"Starting {command}...\r\n");
                var success = await _terminal.StartAsync(command, workingDirectory, 80, 24);
                
                if (success && _terminal.OutputStream != null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    
                    _ = Task.Run(() => ReadOutputAsync(_cancellationTokenSource.Token));
                    
                    AppendOutput("Terminal started successfully. Type 'exit' to close.\r\n");
                }
                else
                {
                    AppendOutput("Failed to start terminal.\r\n");
                }
            }
            catch (Exception ex)
            {
                AppendOutput($"Failed to start terminal: {ex.Message}\r\n");
            }
        }

        private void StopTerminal()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _terminal?.Kill();
                _terminal?.Dispose();
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
                await Dispatcher.InvokeAsync(() => AppendOutput($"\r\nError reading output: {ex.Message}\r\n"));
            }
        }

        private void AppendOutput(string text)
        {
            _outputBuffer.Append(text);
            _outputTextBox.Text = _outputBuffer.ToString();
            _scrollViewer.ScrollToEnd();
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
            }
        }

        private void OnProcessExited(object? sender, int exitCode)
        {
            Dispatcher.InvokeAsync(() =>
            {
                AppendOutput($"\r\nProcess exited with code: {exitCode}\r\n");
                _inputTextBox.IsEnabled = false;
            });
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            _inputTextBox?.Focus();
        }
    }
}