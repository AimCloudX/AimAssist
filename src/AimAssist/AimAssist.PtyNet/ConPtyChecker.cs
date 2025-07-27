using System;
using System.Runtime.InteropServices;

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

        private static bool IsRunningAsAdmin()
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