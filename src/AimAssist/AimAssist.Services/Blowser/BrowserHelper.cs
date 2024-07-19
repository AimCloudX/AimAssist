using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Services.Blowser
{
    public class BrowserHelper
    {
        [DllImport("shell32.dll")]
        static extern int FindExecutable(string lpFile, string lpDirectory, [Out] StringBuilder lpResult);

        public static string GetDefaultBrowserPath()
        {
            string browser = string.Empty;
            try
            {
                string testUrl = "http://example.com";
                StringBuilder path = new StringBuilder(1024);
                int result = FindExecutable(testUrl, "", path);

                if (result > 32)
                {
                    browser = path.ToString();
                }
                else
                {
                    // FindExecutableが失敗した場合、レジストリから取得を試みる
                    using (RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false))
                    {
                        if (key != null)
                        {
                            object val = key.GetValue(null);
                            if (val != null)
                            {
                                browser = val.ToString().ToLower();
                                // コマンドライン引数を削除
                                if (browser.StartsWith("\""))
                                {
                                    browser = browser.Substring(1, browser.IndexOf("\"", 1) - 1);
                                }
                                else
                                {
                                    int index = browser.IndexOf(".exe");
                                    if (index > 0)
                                    {
                                        browser = browser.Substring(0, index + 4);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting default browser: {ex.Message}");
            }

            return browser;
        }

        public static string GetDefaultBrowserName()
        {
            string browserPath = GetDefaultBrowserPath();
            if (!string.IsNullOrEmpty(browserPath))
            {
                return System.IO.Path.GetFileNameWithoutExtension(browserPath);
            }
            return string.Empty;
        }
    }
}
