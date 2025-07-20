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
    public class ConPTYTerminalFixed : IDisposable
    {
        private IntPtr _hPC = IntPtr.Zero;
        private SafeFileHandle? _inputPipeRead;
        private SafeFileHandle? _outputPipeWrite;
        private SafeFileHandle? _inputPipeWrite;
        private SafeFileHandle? _outputPipeRead;
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
                System.Diagnostics.Debug.WriteLine($"Starting ConPTY (Fixed) with command: {commandLine}");

                Columns = cols;
                Rows = rows;

                // Microsoft公式パターン：パイプを作成
                var securityAttributes = new SECURITY_ATTRIBUTES();
                securityAttributes.nLength = Marshal.SizeOf(securityAttributes);
                securityAttributes.bInheritHandle = 1;

                // Input pipe: Parent writes, Child reads
                if (!CreatePipe(out _inputPipeRead, out _inputPipeWrite, ref securityAttributes, 0))
                {
                    System.Diagnostics.Debug.WriteLine("Failed to create input pipe");
                    return false;
                }

                // Output pipe: Child writes, Parent reads  
                if (!CreatePipe(out _outputPipeRead, out _outputPipeWrite, ref securityAttributes, 0))
                {
                    System.Diagnostics.Debug.WriteLine("Failed to create output pipe");
                    return false;
                }

                // 親プロセス側のハンドルは継承不可に設定
                SetHandleInformation(_inputPipeWrite, HANDLE_FLAG_INHERIT, 0);
                SetHandleInformation(_outputPipeRead, HANDLE_FLAG_INHERIT, 0);

                System.Diagnostics.Debug.WriteLine("Pipes created successfully");

                // ConPTYを作成（子プロセス側のハンドルを使用）
                var consoleSize = new COORD((short)cols, (short)rows);
                int result = CreatePseudoConsole(consoleSize, _inputPipeRead, _outputPipeWrite, 0, out _hPC);
                if (result != 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to create ConPTY, error: {result}");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine("ConPTY created successfully");

                // プロセス属性リストを準備
                IntPtr attributeListSize = IntPtr.Zero;
                InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref attributeListSize);

                var attributeList = Marshal.AllocHGlobal(attributeListSize);
                if (!InitializeProcThreadAttributeList(attributeList, 1, 0, ref attributeListSize))
                {
                    Marshal.FreeHGlobal(attributeList);
                    return false;
                }

                // ConPTYハンドルを属性リストに追加
                if (!UpdateProcThreadAttribute(
                    attributeList,
                    0,
                    PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE,
                    _hPC,
                    (IntPtr)IntPtr.Size,
                    IntPtr.Zero,
                    IntPtr.Zero))
                {
                    DeleteProcThreadAttributeList(attributeList);
                    Marshal.FreeHGlobal(attributeList);
                    return false;
                }

                // プロセスを開始
                var processAttributes = new SECURITY_ATTRIBUTES();
                var threadAttributes = new SECURITY_ATTRIBUTES();
                var startupInfoEx = new STARTUPINFOEX
                {
                    lpAttributeList = attributeList
                };
                startupInfoEx.StartupInfo.cb = (uint)Marshal.SizeOf(typeof(STARTUPINFOEX));

                bool success = CreateProcess(
                    null,
                    commandLine,
                    ref processAttributes,
                    ref threadAttributes,
                    true,
                    EXTENDED_STARTUPINFO_PRESENT,
                    IntPtr.Zero,
                    workingDirectory,
                    ref startupInfoEx,
                    out _processInfo);

                DeleteProcThreadAttributeList(attributeList);
                Marshal.FreeHGlobal(attributeList);

                if (!success)
                {
                    int lastError = Marshal.GetLastWin32Error();
                    System.Diagnostics.Debug.WriteLine($"Failed to create process, error: {lastError}");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"Process created successfully, PID: {_processInfo.dwProcessId}");

                // 子プロセス側のハンドルを閉じる（重要）
                CloseHandle(_processInfo.hThread);
                _inputPipeRead?.Close();
                _outputPipeWrite?.Close();

                // 出力読み取りを開始
                _cancellationTokenSource = new CancellationTokenSource();
                _ = Task.Run(async () => await ReadOutputAsync(_cancellationTokenSource.Token));
                _ = Task.Run(async () => await WaitForProcessExitAsync(_cancellationTokenSource.Token));

                System.Diagnostics.Debug.WriteLine("ConPTY terminal (Fixed) started successfully");
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
            if (_outputPipeRead == null || _outputPipeRead.IsInvalid)
            {
                System.Diagnostics.Debug.WriteLine("Output pipe is null or invalid");
                return;
            }

            System.Diagnostics.Debug.WriteLine("Starting output reading (Fixed)...");

            try
            {
                await Task.Run(() =>
                {
                    var buffer = new byte[4096];
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            bool success = ReadFile(_outputPipeRead, buffer, (uint)buffer.Length, out uint bytesRead, IntPtr.Zero);

                            if (success && bytesRead > 0)
                            {
                                string output = Encoding.UTF8.GetString(buffer, 0, (int)bytesRead);
                                System.Diagnostics.Debug.WriteLine($"Received output (Fixed): {output.Replace("\r", "\\r").Replace("\n", "\\n")}");
                                
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
                                Thread.Sleep(10);
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
            }
        }

        public async Task WriteInputAsync(string input)
        {
            if (_inputPipeWrite == null || _inputPipeWrite.IsInvalid || !IsRunning)
                return;

            try
            {
                await Task.Run(() =>
                {
                    byte[] data = Encoding.UTF8.GetBytes(input);
                    bool success = WriteFile(_inputPipeWrite, data, (uint)data.Length, out uint bytesWritten, IntPtr.Zero);

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
                _inputPipeWrite?.Close();
                _outputPipeRead?.Close();
                _inputPipeRead?.Close();
                _outputPipeWrite?.Close();

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