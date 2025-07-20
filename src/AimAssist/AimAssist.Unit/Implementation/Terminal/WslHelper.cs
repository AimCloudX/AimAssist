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
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8 // Use UTF-8 encoding
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    var outputTask = process.StandardOutput.ReadToEndAsync();
                    var errorTask = process.StandardError.ReadToEndAsync();
                    
                    // Add timeout for the process
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(10));
                    var completedTask = await Task.WhenAny(Task.WhenAll(outputTask, errorTask), timeoutTask);
                    
                    if (completedTask == timeoutTask)
                    {
                        process.Kill();
                        Console.WriteLine("WSL command timed out");
                        return distributions;
                    }

                    await process.WaitForExitAsync();
                    
                    if (process.ExitCode != 0)
                    {
                        var error = await errorTask;
                        Console.WriteLine($"WSL command failed with exit code {process.ExitCode}: {error}");
                        return distributions;
                    }

                    var output = await outputTask;
                    Console.WriteLine($"WSL output: {output}"); // Debug output

                    // Parse WSL output - handle different encodings
                    var lines = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    // Skip header line and process each distribution line
                    bool foundHeader = false;
                    foreach (var line in lines)
                    {
                        var trimmedLine = line.Trim();
                        
                        // Skip header line (contains "NAME", "STATE", "VERSION")
                        if (!foundHeader && (trimmedLine.Contains("NAME") || trimmedLine.Contains("名前")))
                        {
                            foundHeader = true;
                            continue;
                        }
                        
                        if (!foundHeader || string.IsNullOrWhiteSpace(trimmedLine))
                            continue;

                        // Clean up any non-printable characters
                        var cleanLine = Regex.Replace(trimmedLine, @"[\u0000-\u001F\u007F-\u009F]", "");
                        var parts = Regex.Split(cleanLine, @"\s+");
                        
                        if (parts.Length >= 2)
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
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            return distributions;
        }

        // Keep synchronous version for fallback
        public static List<WslDistribution> GetInstalledDistributions()
        {
            try
            {
                // Use a longer timeout to prevent hanging
                var task = GetInstalledDistributionsAsync();
                if (task.Wait(TimeSpan.FromSeconds(15)))
                {
                    return task.Result;
                }
                else
                {
                    Console.WriteLine("WSL detection timed out");
                    return new List<WslDistribution>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to get WSL distributions (sync): {ex.Message}");
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
                    Arguments = "--status",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    // Use timeout to prevent hanging
                    if (process.WaitForExit(5000)) // 5 second timeout
                    {
                        return process.ExitCode == 0;
                    }
                    else
                    {
                        try
                        {
                            process.Kill(); // Kill if timeout
                        }
                        catch { }
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WSL installation check failed: {ex.Message}");
            }
            
            return false;
        }

        // Alternative method to check WSL using version command
        public static async Task<bool> IsWslInstalledAsync()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "wsl.exe",
                    Arguments = "--version",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process != null)
                {
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
                    var processTask = process.WaitForExitAsync();
                    
                    var completedTask = await Task.WhenAny(processTask, timeoutTask);
                    
                    if (completedTask == timeoutTask)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch { }
                        return false;
                    }

                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"WSL async installation check failed: {ex.Message}");
            }
            
            return false;
        }
    }
}