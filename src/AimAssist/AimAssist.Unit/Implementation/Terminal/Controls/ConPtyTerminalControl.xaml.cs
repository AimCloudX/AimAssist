using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AimAssist.Units.Implementation.Terminal.ConPTY;

namespace AimAssist.Units.Implementation.Terminal.Controls
{
    public partial class ConPtyTerminalControl : UserControl, IDisposable
    {
        private ConPTYTerminalFixed? _terminal;
        private bool _disposed = false;

        public string Title { get; set; } = "ConPTY Terminal";
        public string Shell { get; set; } = "pwsh.exe";

        public event EventHandler? ProcessExited;

        public ConPtyTerminalControl()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Loadedイベントでは自動開始しない（明示的にStartTerminalAsyncを呼ぶ）
        }

        public async System.Threading.Tasks.Task StartTerminalAsync()
        {
            if (_terminal != null)
                return;

            try
            {
                outputTextBlock.Text = $"Starting ConPTY terminal: {Shell}...\r\n";
                
                _terminal = new ConPTYTerminalFixed();
                _terminal.OutputReceived += OnOutputReceived;
                _terminal.ProcessExited += OnProcessExited;

                // ターミナルサイズを計算
                int cols = Math.Max(80, (int)(ActualWidth / 8));
                int rows = Math.Max(24, (int)(ActualHeight / 16));

                bool success = await _terminal.StartAsync(Shell, null, cols, rows);
                if (success)
                {
                    outputTextBlock.Text += $"ConPTY terminal started successfully!\r\n";
                    
                    // 初期プロンプト表示のため改行を送信
                    await System.Threading.Tasks.Task.Delay(500);
                    await _terminal.WriteInputAsync("\r\n");
                }
                else
                {
                    outputTextBlock.Text += $"Failed to start ConPTY terminal: {Shell}\r\n";
                }
            }
            catch (Exception ex)
            {
                outputTextBlock.Text += $"Error starting ConPTY terminal: {ex.Message}\r\n";
                System.Diagnostics.Debug.WriteLine($"ConPTY terminal start error: {ex}");
            }
        }

        private void OnOutputReceived(object? sender, string output)
        {
            Dispatcher.Invoke(() =>
            {
                // 初期メッセージを削除して実際の出力を表示
                if (outputTextBlock.Text.Contains("Starting ConPTY terminal") || 
                    outputTextBlock.Text.Contains("ConPTY terminal started successfully"))
                {
                    outputTextBlock.Text = "";
                }
                
                outputTextBlock.Text += output;
                
                // 自動スクロール
                outputScrollViewer.ScrollToEnd();
                
                // 出力が長すぎる場合は古い部分を削除
                if (outputTextBlock.Text.Length > 50000)
                {
                    string text = outputTextBlock.Text;
                    int newStartIndex = text.Length - 40000;
                    outputTextBlock.Text = text.Substring(newStartIndex);
                }
            });
        }

        private void OnProcessExited(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                outputTextBlock.Text += "\r\n[Process Exited]\r\n";
                ProcessExited?.Invoke(this, EventArgs.Empty);
            });
        }

        private async void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await SendInput();
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                // Tab補完のため、Tabキーもターミナルに送信
                await SendTabInput();
                e.Handled = true;
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendInput();
        }

        private async System.Threading.Tasks.Task SendInput()
        {
            if (_terminal == null || string.IsNullOrEmpty(inputTextBox.Text))
                return;

            string input = inputTextBox.Text + "\r\n";
            await _terminal.WriteInputAsync(input);
            
            inputTextBox.Clear();
            inputTextBox.Focus();
        }

        private async System.Threading.Tasks.Task SendTabInput()
        {
            if (_terminal == null)
                return;

            string currentInput = inputTextBox.Text + "\t";
            await _terminal.WriteInputAsync(currentInput);
        }

        public void ResizeTerminal(int width, int height)
        {
            if (_terminal == null)
                return;

            int cols = Math.Max(80, width / 8);
            int rows = Math.Max(24, height / 16);
            
            _terminal.Resize(cols, rows);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            
            if (sizeInfo.NewSize.Width > 0 && sizeInfo.NewSize.Height > 0)
            {
                ResizeTerminal((int)sizeInfo.NewSize.Width, (int)sizeInfo.NewSize.Height);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _terminal?.Dispose();
        }
    }
}