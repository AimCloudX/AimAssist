using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AimAssist.Units.Implementation.Terminal
{
    public class SimpleTerminal : IDisposable
    {
        private Process? _process;
        private StreamWriter? _inputWriter;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _disposed = false;

        public event EventHandler<string>? OutputReceived;
        public event EventHandler? ProcessExited;

        public bool IsRunning => _process != null && !_process.HasExited;

        public Task<bool> StartAsync(string shell = "pwsh.exe", string? workingDirectory = null)
        {
            return Task.Run(() => Start(shell, workingDirectory));
        }

        private bool Start(string shell = "pwsh.exe", string? workingDirectory = null)
        {
            try
            {
                if (_process != null && !_process.HasExited)
                {
                    return false;
                }

                _cancellationTokenSource = new CancellationTokenSource();

                // PowerShell Coreが利用できない場合はWindows PowerShellにフォールバック
                if (shell.Equals("pwsh.exe", StringComparison.OrdinalIgnoreCase))
                {
                    // まずPowerShell Core (pwsh.exe)を探す
                    var pwshPath = GetExecutablePath("pwsh.exe");
                    if (!string.IsNullOrEmpty(pwshPath))
                    {
                        shell = pwshPath;
                    }
                    else
                    {
                        // PowerShell Coreが見つからない場合はWindows PowerShellを使用
                        shell = "powershell.exe";
                    }
                }

                // WSLの場合、コマンドライン引数を分離
                string fileName = shell;
                string arguments = "";
                
                if (shell.Contains(" "))
                {
                    var parts = shell.Split(' ', 2);
                    fileName = parts[0];
                    arguments = parts.Length > 1 ? parts[1] : "";
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    WorkingDirectory = workingDirectory ?? Environment.CurrentDirectory
                };

                // シェルごとの設定
                if (fileName.EndsWith("cmd.exe", StringComparison.OrdinalIgnoreCase))
                {
                    startInfo.Arguments = "/K " + arguments; // Keep window open
                }
                else if (fileName.EndsWith("pwsh.exe", StringComparison.OrdinalIgnoreCase) || 
                         fileName.EndsWith("powershell.exe", StringComparison.OrdinalIgnoreCase))
                {
                    // PowerShellは特別な引数なしで起動（リダイレクトと相性が悪い）
                    // 既存の引数をそのまま使用
                }
                else if (fileName.EndsWith("wsl.exe", StringComparison.OrdinalIgnoreCase))
                {
                    // WSLの引数はすでに設定済み（-d DistroNameなど）
                }

                _process = new Process { StartInfo = startInfo };
                _process.EnableRaisingEvents = true;
                _process.Exited += OnProcessExited;

                if (!_process.Start())
                {
                    Console.WriteLine($"Failed to start process: {shell}");
                    return false;
                }

                _inputWriter = _process.StandardInput;

                // 標準出力の読み取りを開始
                _ = Task.Run(async () => await ReadOutputAsync(_process.StandardOutput, _cancellationTokenSource.Token));
                _ = Task.Run(async () => await ReadOutputAsync(_process.StandardError, _cancellationTokenSource.Token));

                // 初期メッセージを送信して、シェルの準備ができたことを確認
                Console.WriteLine($"Successfully started: {shell}");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start terminal ({shell}): {ex.Message}");
                Debug.WriteLine($"Shell command: {shell}");
                Debug.WriteLine($"Exception: {ex}");
                return false;
            }
        }

        private async Task ReadOutputAsync(StreamReader reader, CancellationToken cancellationToken)
        {
            try
            {
                var buffer = new char[1024];
                while (!cancellationToken.IsCancellationRequested && !reader.EndOfStream)
                {
                    var count = await reader.ReadAsync(buffer, 0, buffer.Length);
                    if (count > 0)
                    {
                        var output = new string(buffer, 0, count);
                        OutputReceived?.Invoke(this, output);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading output: {ex.Message}");
            }
        }

        public async Task WriteInputAsync(string input)
        {
            if (_inputWriter == null || !IsRunning)
                return;

            try
            {
                // WriteLineAsyncを使用して改行を自動的に追加
                await _inputWriter.WriteLineAsync(input);
                await _inputWriter.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing input: {ex.Message}");
                Debug.WriteLine($"Input write error: {ex}");
            }
        }

        private void OnProcessExited(object? sender, EventArgs e)
        {
            ProcessExited?.Invoke(this, EventArgs.Empty);
        }

        private static string? GetExecutablePath(string fileName)
        {
            try
            {
                var paths = Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) ?? Array.Empty<string>();
                foreach (var path in paths)
                {
                    var fullPath = Path.Combine(path, fileName);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }
            catch { }
            return null;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _cancellationTokenSource?.Cancel();

            try
            {
                _inputWriter?.Close();
                _inputWriter?.Dispose();
                
                if (_process != null && !_process.HasExited)
                {
                    _process.Kill();
                }
                _process?.Dispose();
            }
            catch { }

            _cancellationTokenSource?.Dispose();
        }
    }
}