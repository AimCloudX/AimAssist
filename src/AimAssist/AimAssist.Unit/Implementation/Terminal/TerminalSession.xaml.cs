using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AimAssist.Units.Implementation.Terminal
{
    public partial class TerminalSession : UserControl
    {
        private SimpleTerminal _terminal;
        private List<string> _commandHistory;
        private int _historyIndex;
        public string SessionName { get; set; }
        public string ShellType { get; set; }

        public TerminalSession(string sessionName, string shellType)
        {
            InitializeComponent();
            SessionName = sessionName;
            ShellType = shellType;
            _terminal = new SimpleTerminal();
            _commandHistory = new List<string>();
            _historyIndex = -1;

            _terminal.OutputReceived += Terminal_OutputReceived;
            _terminal.ProcessExited += Terminal_ProcessExited;

            // 自動接続
            _ = ConnectAsync();
        }

        private async Task ConnectAsync()
        {
            var success = await _terminal.StartAsync(ShellType);
            if (success)
            {
                txtInput.IsEnabled = true;
                txtInput.Focus();
            }
            else
            {
                txtTerminal.AppendText($"{ShellType} の起動に失敗しました。\r\n");
            }
        }

        private void Terminal_OutputReceived(object? sender, string output)
        {
            Dispatcher.Invoke(() =>
            {
                txtTerminal.AppendText(output);
                scrollViewer.ScrollToEnd();
            });
        }

        private void Terminal_ProcessExited(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                txtInput.IsEnabled = false;
                txtTerminal.AppendText("\r\n[プロセスが終了しました]\r\n");
            });
        }

        private async void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    await SendInputAsync();
                    e.Handled = true;
                    break;

                case Key.Up:
                    NavigateHistory(-1);
                    e.Handled = true;
                    break;

                case Key.Down:
                    NavigateHistory(1);
                    e.Handled = true;
                    break;
            }
        }

        private async Task SendInputAsync()
        {
            if (string.IsNullOrEmpty(txtInput.Text))
                return;

            try
            {
                string input = txtInput.Text;

                // コマンド履歴に追加
                if (!string.IsNullOrWhiteSpace(input))
                {
                    _commandHistory.Add(input);
                    if (_commandHistory.Count > 100)
                    {
                        _commandHistory.RemoveAt(0);
                    }
                }
                _historyIndex = _commandHistory.Count;

                // コマンドを送信
                await _terminal.WriteInputAsync(input);

                txtInput.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"入力送信エラー: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NavigateHistory(int direction)
        {
            if (_commandHistory.Count == 0)
                return;

            _historyIndex += direction;

            if (_historyIndex < 0)
                _historyIndex = 0;
            else if (_historyIndex >= _commandHistory.Count)
                _historyIndex = _commandHistory.Count;

            if (_historyIndex < _commandHistory.Count)
            {
                txtInput.Text = _commandHistory[_historyIndex];
                txtInput.CaretIndex = txtInput.Text.Length;
            }
            else
            {
                txtInput.Clear();
            }
        }

        public void Dispose()
        {
            _terminal?.Dispose();
        }
    }
}