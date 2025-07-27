using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace AimAssist.PtyNet
{
    public class ConPtyTerminal : IDisposable
    {
        private const int PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE = 0x00020016;
        private const uint EXTENDED_STARTUPINFO_PRESENT = 0x00080000;

        private IntPtr _hPC = IntPtr.Zero;
        private SafeFileHandle? _hPipeIn;
        private SafeFileHandle? _hPipeOut;
        private Process? _childProcess;
        private bool _disposed;

        public event EventHandler<int>? ProcessExited;
        public Stream? InputStream { get; private set; }
        public Stream? OutputStream { get; private set; }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int CreatePseudoConsole(COORD size, IntPtr hInput, IntPtr hOutput, uint dwFlags, out IntPtr phPC);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int ResizePseudoConsole(IntPtr hPC, COORD size);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void ClosePseudoConsole(IntPtr hPC);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CreatePipe(out SafeFileHandle hReadPipe, out SafeFileHandle hWritePipe, IntPtr lpPipeAttributes, uint nSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, uint dwFlags, ref IntPtr lpSize);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, uint dwFlags, IntPtr Attribute, IntPtr lpValue, IntPtr cbSize, IntPtr lpPreviousValue, IntPtr lpReturnSize);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool CreateProcess(string? lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string? lpCurrentDirectory, ref STARTUPINFOEX lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern void DeleteProcThreadAttributeList(IntPtr lpAttributeList);

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            public short X;
            public short Y;

            public COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFOEX
        {
            public STARTUPINFO StartupInfo;
            public IntPtr lpAttributeList;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct STARTUPINFO
        {
            public int cb;
            public string? lpReserved;
            public string? lpDesktop;
            public string? lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        public async Task<bool> StartAsync(string command = "cmd.exe", string? workingDirectory = null, int cols = 80, int rows = 24)
        {
            if (_disposed) return false;

            try
            {
                // Create pipes for communication
                if (!CreatePipe(out var hPipeInRead, out var hPipeInWrite, IntPtr.Zero, 0) ||
                    !CreatePipe(out var hPipeOutRead, out var hPipeOutWrite, IntPtr.Zero, 0))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                _hPipeIn = hPipeInWrite;
                _hPipeOut = hPipeOutRead;

                // Create the pseudoconsole
                var consoleSize = new COORD((short)cols, (short)rows);
                var hr = CreatePseudoConsole(consoleSize, hPipeInRead.DangerousGetHandle(), hPipeOutWrite.DangerousGetHandle(), 0, out _hPC);
                if (hr != 0)
                {
                    throw new Win32Exception(hr);
                }

                // Close the handles we passed to the pseudoconsole
                hPipeInRead.Close();
                hPipeOutWrite.Close();

                // Create streams
                InputStream = new FileStream(_hPipeIn, FileAccess.Write);
                OutputStream = new FileStream(_hPipeOut, FileAccess.Read);

                // Start the process
                await StartProcessAsync(command, workingDirectory);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to start ConPty terminal: {ex.Message}");
                return false;
            }
        }

        private async Task StartProcessAsync(string command, string? workingDirectory)
        {
            var startupInfo = new STARTUPINFOEX();
            startupInfo.StartupInfo.cb = Marshal.SizeOf<STARTUPINFOEX>();

            // Calculate the size needed for the attribute list
            var attributeListSize = IntPtr.Zero;
            InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref attributeListSize);

            // Allocate memory for the attribute list
            startupInfo.lpAttributeList = Marshal.AllocHGlobal(attributeListSize);

            try
            {
                // Initialize the attribute list
                if (!InitializeProcThreadAttributeList(startupInfo.lpAttributeList, 1, 0, ref attributeListSize))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                // Set the pseudoconsole attribute
                if (!UpdateProcThreadAttribute(startupInfo.lpAttributeList, 0, (IntPtr)PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE, _hPC, (IntPtr)IntPtr.Size, IntPtr.Zero, IntPtr.Zero))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                // Create the process
                if (!CreateProcess(null, command, IntPtr.Zero, IntPtr.Zero, false, EXTENDED_STARTUPINFO_PRESENT, IntPtr.Zero, workingDirectory, ref startupInfo, out var processInfo))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                // Create a Process object for easier management
                _childProcess = Process.GetProcessById(processInfo.dwProcessId);
                _childProcess.EnableRaisingEvents = true;
                _childProcess.Exited += OnProcessExited;

                // Close process and thread handles
                CloseHandle(processInfo.hProcess);
                CloseHandle(processInfo.hThread);
            }
            finally
            {
                if (startupInfo.lpAttributeList != IntPtr.Zero)
                {
                    DeleteProcThreadAttributeList(startupInfo.lpAttributeList);
                    Marshal.FreeHGlobal(startupInfo.lpAttributeList);
                }
            }
        }

        private void OnProcessExited(object? sender, EventArgs e)
        {
            if (_childProcess != null)
            {
                ProcessExited?.Invoke(this, _childProcess.ExitCode);
            }
        }

        public void Resize(int cols, int rows)
        {
            if (_hPC != IntPtr.Zero)
            {
                var consoleSize = new COORD((short)cols, (short)rows);
                ResizePseudoConsole(_hPC, consoleSize);
            }
        }

        public void Kill()
        {
            try
            {
                _childProcess?.Kill();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error killing process: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _childProcess?.Kill();
                _childProcess?.Dispose();
            }
            catch { }

            try
            {
                InputStream?.Dispose();
                OutputStream?.Dispose();
            }
            catch { }

            try
            {
                _hPipeIn?.Dispose();
                _hPipeOut?.Dispose();
            }
            catch { }

            if (_hPC != IntPtr.Zero)
            {
                ClosePseudoConsole(_hPC);
                _hPC = IntPtr.Zero;
            }

            _disposed = true;
        }
    }
}