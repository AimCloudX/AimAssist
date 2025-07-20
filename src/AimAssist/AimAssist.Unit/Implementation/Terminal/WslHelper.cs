using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace AimAssist.Units.Implementation.Terminal
{
    public static class WslHelper
    {
        public class WslDistribution
        {
            public string Name { get; set; } = string.Empty;
            public bool IsDefault { get; set; }
            public string State { get; set; } = string.Empty;
            public int Version { get; set; }
        }

        public static async Task<List<WslDistribution>> GetInstalledDistributionsAsync()
        {
            var distributions = new List<WslDistribution>();

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = "--list --verbose",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.Unicode // Windows uses UTF-16 for wsl.exe output
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    var output = await process.StandardOutput.ReadToEndAsync();
                    await process.WaitForExitAsync();

                    // Parse WSL output
                    var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                    
                    // Skip header line
                    foreach (var line in lines.Skip(1))
                    {
                        var parts = Regex.Split(line.Trim(), @"\s+");
                        if (parts.Length >= 3)
                        {
                            var name = parts[0];
                            var isDefault = false;
                            
                            // Check if this is the default distribution
                            if (name.StartsWith("*"))
                            {
                                isDefault = true;
                                name = name.Substring(1).Trim();
                            }

                            // Skip if empty name
                            if (string.IsNullOrWhiteSpace(name))
                                continue;

                            var distribution = new WslDistribution
                            {
                                Name = name,
                                IsDefault = isDefault,
                                State = parts.Length > 1 ? parts[1] : "Unknown",
                                Version = parts.Length > 2 && int.TryParse(parts[2], out var ver) ? ver : 2
                            };

                            distributions.Add(distribution);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get WSL distributions: {ex.Message}");
            }

            return distributions;
        }

        // Keep synchronous version for fallback
        public static List<WslDistribution> GetInstalledDistributions()
        {
            try
            {
                // Use a timeout to prevent hanging
                var task = GetInstalledDistributionsAsync();
                if (task.Wait(TimeSpan.FromSeconds(5)))
                {
                    return task.Result;
                }
                else
                {
                    Console.WriteLine("WSL detection timed out");
                    return new List<WslDistribution>();
                }
            }
            catch
            {
                return new List<WslDistribution>();
            }
        }

        public static bool IsWslInstalled()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = "--help",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    // Use timeout to prevent hanging
                    if (process.WaitForExit(3000)) // 3 second timeout
                    {
                        return process.ExitCode == 0;
                    }
                    else
                    {
                        process.Kill(); // Kill if timeout
                        return false;
                    }
                }
            }
            catch { }
            
            return false;
        }
    }
}