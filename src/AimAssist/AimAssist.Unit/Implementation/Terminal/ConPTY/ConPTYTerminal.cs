using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;
using static AimAssist.Units.Implementation.Terminal.ConPTY.ConPTYAPI;

namespace AimAssist.Units.Implementation.Terminal.ConPTY
{
    public class ConPTYTerminal : IDisposable
    {
        private IntPtr _hPC = IntPtr.Zero;
        private SafeFileHandle? _inputPipeOurSide;
        private SafeFileHandle? _outputPipeOurSide;
        private SafeFileHandle? _inputPipeTheirSide;
        private SafeFileHandle? _outputPipeTheirSide;
        private PROCESS_INFORMATION _processInfo;
        private CancellationTokenSource? _cancellationTokenSource;
        private bool _disposed = false;

        public event EventHandler<string>? OutputReceived;
        public event EventHandler? ProcessExited;

        public bool IsRunning => _processInfo.hProcess != IntPtr.Zero;

        public int Columns { get; private set; } = 80;
        public int Rows { get; private set; } = 24;

        public async Task<bool> StartAsync(string commandLine = "pwsh.exe", string? workingDirectory = null, int cols = 80, int rows = 24)
        {
            return await Task.Run(() => Start(commandLine, workingDirectory, cols, rows));
        }

        private bool Start(string commandLine, string? workingDirectory, int cols, int rows)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Starting ConPTY with command: {commandLine}");
                
                Columns = cols;
                Rows = rows;

                // パイプを作成（継承可能に設定）
                var security = new SECURITY_ATTRIBUTES
                {
                    nLength = Marshal.SizeOf(typeof(SECURITY_ATTRIBUTES)),
                    lpSecurityDescriptor = IntPtr.Zero,
                    bInheritHandle = 1  // 継承可能
                };
                
                if (!CreatePipe(out _inputPipeOurSide, out _inputPipeTheirSide, ref security, 0))
                {
                    System.Diagnostics.Debug.WriteLine("Failed to create input pipe");
                    return false;
                }
                
                if (!CreatePipe(out _outputPipeTheirSide, out _outputPipeOurSide, ref security, 0))
                {
                    System.Diagnostics.Debug.WriteLine("Failed to create output pipe");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine("Pipes created successfully");

                // 子プロセス側のハンドルを継承可能に設定
                SetHandleInformation(_inputPipeTheirSide, HANDLE_FLAG_INHERIT, HANDLE_FLAG_INHERIT);
                SetHandleInformation(_outputPipeTheirSide, HANDLE_FLAG_INHERIT, HANDLE_FLAG_INHERIT);
                
                // 親プロセス側のハンドルは継承しない
                SetHandleInformation(_inputPipeOurSide, HANDLE_FLAG_INHERIT, 0);
                SetHandleInformation(_outputPipeOurSide, HANDLE_FLAG_INHERIT, 0);

                // ConPTYを作成
                var consoleSize = new COORD((short)cols, (short)rows);
                int result = CreatePseudoConsole(consoleSize, _inputPipeTheirSide, _outputPipeTheirSide, 0, out _hPC);
                if (result != 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create ConPTY, error: {result}");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine("ConPTY created successfully");

                // プロセス属性リストのサイズを取得
                IntPtr lpSize = IntPtr.Zero;
                InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref lpSize);

                // プロセス属性リストを作成
                var lpAttributeList = Marshal.AllocHGlobal(lpSize);
                if (!InitializeProcThreadAttributeList(lpAttributeList, 1, 0, ref lpSize))
                {
                    Marshal.FreeHGlobal(lpAttributeList);
                    return false;
                }

                // ConPTYハンドルを属性リストに追加
                if (!UpdateProcThreadAttribute(
                    lpAttributeList,
                    0,
                    PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE,
                    _hPC,
                    (IntPtr)IntPtr.Size,
                    IntPtr.Zero,
                    IntPtr.Zero))
                {
                    DeleteProcThreadAttributeList(lpAttributeList);
                    Marshal.FreeHGlobal(lpAttributeList);
                    return false;
                }

                // プロセスを開始
                var processAttributes = new SECURITY_ATTRIBUTES();
                var threadAttributes = new SECURITY_ATTRIBUTES();
                var startupInfoEx = new STARTUPINFOEX
                {
                    lpAttributeList = lpAttributeList
                };
                startupInfoEx.StartupInfo.cb = (uint)Marshal.SizeOf(typeof(STARTUPINFOEX));

                bool success = CreateProcess(
                    null,
                    commandLine,
                    ref processAttributes,
                    ref threadAttributes,
                    true,  // ハンドルを継承
                    EXTENDED_STARTUPINFO_PRESENT | CREATE_UNICODE_ENVIRONMENT,
                    IntPtr.Zero,
                    workingDirectory,
                    ref startupInfoEx,
                    out _processInfo);

                DeleteProcThreadAttributeList(lpAttributeList);
                Marshal.FreeHGlobal(lpAttributeList);

                if (!success)
                {
                    int lastError = Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"Failed to create process, error: {lastError}");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Process created successfully, PID: {_processInfo.dwProcessId}");

                // 不要なハンドルをクローズ
                CloseHandle(_processInfo.hThread);
                _inputPipeTheirSide?.Close();
                _outputPipeTheirSide?.Close();

                // FileStreamは使用せず、直接Win32 APIを使用

                // 出力読み取りを開始
                _cancellationTokenSource = new CancellationTokenSource();
                _ = Task.Run(async () => await ReadOutputAsync(_cancellationTokenSource.Token));
                _ = Task.Run(async () => await WaitForProcessExitAsync(_cancellationTokenSource.Token));

                System.Diagnostics.Debug.WriteLine("ConPTY terminal started successfully");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConPTY start exception: {ex}");
                return false;
            }
        }

        private async Task ReadOutputAsync(CancellationToken cancellationToken)
        {
            if (_outputPipeOurSide == null || _outputPipeOurSide.IsInvalid)
            {
                System.Diagnostics.Debug.WriteLine("Output pipe is null or invalid");
                return;
            }

            System.Diagnostics.Debug.WriteLine("Starting output reading...");

            try
            {
                await Task.Run(() =>
                {
                    var buffer = new byte[4096];
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            // Win32 ReadFileを直接使用
                            bool success = ReadFile(_outputPipeOurSide, buffer, (uint)buffer.Length, out uint bytesRead, IntPtr.Zero);
                            
                            if (success && bytesRead > 0)
                            {
                                string output = Encoding.UTF8.GetString(buffer, 0, (int)bytesRead);
                                System.Diagnostics.Debug.WriteLine($"Received output: {output.Replace("\r", "\\r").Replace("\n", "\\n")}");
                                
                                // UIスレッドで出力イベントを発火
                                OutputReceived?.Invoke(this, output);
                            }
                            else if (!success)
                            {
                                int error = Marshal.GetLastWin32Error();
                                if (error == 109) // ERROR_BROKEN_PIPE
                                {
                                    System.Diagnostics.Debug.WriteLine("Pipe broken, ending read loop");
                                    break;
                                }
                                else if (error != 0)
                                {
                                    System.Diagnostics.Debug.WriteLine($"ReadFile error: {error}");
                                    break;
                                }
                            }
                            else if (bytesRead == 0)
                            {
                                System.Diagnostics.Debug.WriteLine("No data read, continuing...");
                                Thread.Sleep(10); // 短時間待機
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Output reading error: {ex.Message}");
                            break;
                        }
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Output reading cancelled");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Output reading error: {ex}");
            }
        }

        private async Task WaitForProcessExitAsync(CancellationToken cancellationToken)
        {
            if (_processInfo.hProcess == IntPtr.Zero) return;

            try
            {
                await Task.Run(() =>
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        uint result = WaitForSingleObject(_processInfo.hProcess, 1000);
                        if (result == 0) // WAIT_OBJECT_0
                        {
                            ProcessExited?.Invoke(this, EventArgs.Empty);
                            break;
                        }
                    }
                }, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // キャンセルされた場合は正常終了
            }
        }

        public async Task WriteInputAsync(string input)
        {
            if (_inputPipeOurSide == null || _inputPipeOurSide.IsInvalid || !IsRunning)
                return;

            try
            {
                await Task.Run(() =>
                {
                    byte[] data = Encoding.UTF8.GetBytes(input);
                    bool success = WriteFile(_inputPipeOurSide, data, (uint)data.Length, out uint bytesWritten, IntPtr.Zero);
                    
                    if (!success)
                    {
                        int error = Marshal.GetLastWin32Error();
                        System.Diagnostics.Debug.WriteLine($"WriteFile error: {error}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Wrote {bytesWritten} bytes to input pipe");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Write input error: {ex.Message}");
            }
        }

        public void Resize(int cols, int rows)
        {
            if (_hPC != IntPtr.Zero)
            {
                Columns = cols;
                Rows = rows;
                var size = new COORD((short)cols, (short)rows);
                ResizePseudoConsole(_hPC, size);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _cancellationTokenSource?.Cancel();

            try
            {
                _inputPipeOurSide?.Close();
                _outputPipeOurSide?.Close();
                _inputPipeTheirSide?.Close();
                _outputPipeTheirSide?.Close();

                if (_processInfo.hProcess != IntPtr.Zero)
                {
                    CloseHandle(_processInfo.hProcess);
                }

                if (_hPC != IntPtr.Zero)
                {
                    ClosePseudoConsole(_hPC);
                }
            }
            catch { }

            _cancellationTokenSource?.Dispose();
        }
    }
}