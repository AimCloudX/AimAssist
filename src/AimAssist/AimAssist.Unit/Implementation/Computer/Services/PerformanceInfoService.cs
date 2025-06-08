using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace AimAssist.Units.Implementation.Computer.Services
{
    public class PerformanceInfoService
    {
        private PerformanceCounter? cpuCounter;
        private PerformanceCounter? ramCounter;
        private bool isInitialized = false;

        public PerformanceInfoService()
        {
        }

        private void InitializeCounters()
        {
            if (isInitialized) return;
            
            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
                cpuCounter.NextValue();
                ramCounter.NextValue();
                isInitialized = true;
            }
            catch
            {
                isInitialized = true;
            }
        }

        public float GetCpuUsage()
        {
            try
            {
                InitializeCounters();
                return cpuCounter?.NextValue() ?? GetCpuUsageAlternative();
            }
            catch
            {
                return GetCpuUsageAlternative();
            }
        }

        public float GetMemoryUsage()
        {
            try
            {
                InitializeCounters();
                return ramCounter?.NextValue() ?? GetMemoryUsageAlternative();
            }
            catch
            {
                return GetMemoryUsageAlternative();
            }
        }

        private float GetCpuUsageAlternative()
        {
            try
            {
                var startTime = DateTime.UtcNow;
                var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
                
                Thread.Sleep(100);
                
                var endTime = DateTime.UtcNow;
                var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
                
                var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
                
                return (float)(cpuUsageTotal * 100);
            }
            catch
            {
                return 0;
            }
        }

        private float GetMemoryUsageAlternative()
        {
            try
            {
                var totalMemory = GC.GetTotalMemory(false);
                var workingSet = Environment.WorkingSet;
                return (float)((double)workingSet / (1024 * 1024 * 1024) * 100);
            }
            catch
            {
                return 0;
            }
        }

        public string GetDiskUsage()
        {
            try
            {
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                    .Take(3);
                
                var results = new List<string>();
                foreach (var drive in drives)
                {
                    var usedPercentage = 100.0 - ((double)drive.AvailableFreeSpace / drive.TotalSize) * 100.0;
                    results.Add($"{drive.Name} {usedPercentage:F1}%");
                }
                
                return results.Any() ? string.Join(", ", results) : "取得できませんでした";
            }
            catch
            {
                return "取得できませんでした";
            }
        }

        public string GetServiceStatus()
        {
            try
            {
                var importantServices = new[] { "Dhcp", "DNSCache" };
                var results = new List<string>();
                
                foreach (var serviceName in importantServices)
                {
                    try
                    {
                        using var sc = new ServiceController(serviceName);
                        results.Add($"{sc.DisplayName}: {sc.Status}");
                    }
                    catch
                    {
                        results.Add($"{serviceName}: 不明");
                    }
                }
                
                return results.Any() ? string.Join(", ", results) : "取得できませんでした";
            }
            catch
            {
                return "取得できませんでした";
            }
        }

        public void Dispose()
        {
            cpuCounter?.Dispose();
            ramCounter?.Dispose();
        }
    }
}
