using System;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace AimAssist.PtyNet
{
    public static class ConPtyChecker
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern int CreatePseudoConsole(COORD size, IntPtr hInput, IntPtr hOutput, uint dwFlags, out IntPtr phPC);

        [StructLayout(LayoutKind.Sequential)]
        private struct COORD
        {
            public short X;
            public short Y;
        }

        public static bool IsConPtyAvailable()
        {
            try
            {
                // Try to get the CreatePseudoConsole function
                var module = GetModuleHandle("kernel32.dll");
                var proc = GetProcAddress(module, "CreatePseudoConsole");
                return proc != IntPtr.Zero;
            }
            catch
            {
                return false;
            }
        }

        public static string GetSystemInfo()
        {
            var osVersion = Environment.OSVersion;
            var isConPtyAvailable = IsConPtyAvailable();
            var currentUser = Environment.UserName;
            var isElevated = IsRunningAsAdmin();

            return $"OS: {osVersion}\n" +
                   $"ConPTY Available: {isConPtyAvailable}\n" +
                   $"Current User: {currentUser}\n" +
                   $"Running as Admin: {isElevated}";
        }

        public static string GetDetailedDiagnostics()
        {
            var diagnostics = GetSystemInfo() + "\n\n";

            // 追加診断情報
            diagnostics += "=== Security Diagnostics ===\n";
            diagnostics += $"UAC Enabled: {IsUacEnabled()}\n";
            diagnostics += $"Process Integrity Level: {GetProcessIntegrityLevel()}\n";
            diagnostics += $"Windows Defender Status: {GetDefenderStatus()}\n";
            diagnostics += $"ConPTY Test Result: {TestConPtyCreation()}\n";

            return diagnostics;
        }

        private static bool IsUacEnabled()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System");
                var value = key?.GetValue("EnableLUA");
                return value?.ToString() == "1";
            }
            catch
            {
                return false;
            }
        }

        private static string GetProcessIntegrityLevel()
        {
            try
            {
                var identity = WindowsIdentity.GetCurrent();
                var principal = new WindowsPrincipal(identity);
                
                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                    return "High (Administrator)";
                else if (identity.Groups != null)
                {
                    foreach (var group in identity.Groups)
                    {
                        if (group.Value.EndsWith("-12288")) // Medium Integrity
                            return "Medium";
                        if (group.Value.EndsWith("-4096"))  // Low Integrity
                            return "Low";
                    }
                }
                return "Medium (Standard User)";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private static string GetDefenderStatus()
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows Defender\Real-Time Protection");
                var disabled = key?.GetValue("DisableRealtimeMonitoring");
                return disabled?.ToString() == "1" ? "Disabled" : "Enabled";
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string TestConPtyCreation()
        {
            try
            {
                // Create test pipes
                if (!CreatePipe(out var hPipeInRead, out var hPipeInWrite, IntPtr.Zero, 0) ||
                    !CreatePipe(out var hPipeOutRead, out var hPipeOutWrite, IntPtr.Zero, 0))
                {
                    return "Failed to create pipes";
                }

                try
                {
                    var size = new COORD { X = 80, Y = 24 };
                    var result = CreatePseudoConsole(size, hPipeInRead.DangerousGetHandle(), hPipeOutWrite.DangerousGetHandle(), 0, out var hPC);
                    
                    if (result == 0 && hPC != IntPtr.Zero)
                    {
                        ClosePseudoConsole(hPC);
                        return "Success";
                    }
                    else
                    {
                        return $"Failed with error code: {result} (0x{result:X})";
                    }
                }
                finally
                {
                    hPipeInRead?.Close();
                    hPipeInWrite?.Close();
                    hPipeOutRead?.Close();
                    hPipeOutWrite?.Close();
                }
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern void ClosePseudoConsole(IntPtr hPC);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CreatePipe(out Microsoft.Win32.SafeHandles.SafeFileHandle hReadPipe, 
            out Microsoft.Win32.SafeHandles.SafeFileHandle hWritePipe, IntPtr lpPipeAttributes, uint nSize);

        public static bool IsRunningAsAdmin()
        {
            try
            {
                var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
    }
}