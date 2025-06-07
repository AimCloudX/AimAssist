using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace AimAssist.Units.Implementation.Computer.Services
{
    public class PerformanceInfoService
    {
        private readonly PerformanceCounter? cpuCounter;
        private readonly PerformanceCounter? ramCounter;

        public PerformanceInfoService()
        {
            try
            {
                cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");
            }
            catch
            {
                // パフォーマンスカウンターの初期化に失敗した場合
            }
        }

        public float GetCpuUsage()
        {
            try
            {
                return cpuCounter?.NextValue() ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public float GetMemoryUsage()
        {
            try
            {
                return ramCounter?.NextValue() ?? 0;
            }
            catch
            {
                return 0;
            }
        }

        public string GetDiskUsage()
        {
            string diskUsage = string.Empty;
            
            try
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                
                foreach (DriveInfo drive in allDrives)
                {
                    if (drive.IsReady)
                    {
                        double usedPercentage = 100.0 - ((double)drive.AvailableFreeSpace / drive.TotalSize) * 100.0;
                        diskUsage += $"ドライブ {drive.Name}: {usedPercentage:F1}%\n";
                    }
                }
            }
            catch
            {
                diskUsage = "情報を取得できませんでした";
            }
            
            return diskUsage;
        }

        public string GetServiceStatus()
        {
            try
            {
                string serviceStatus = string.Empty;
                
                string[] importantServices = { "Dhcp", "DNSCache", "BITS", "wuauserv" };
                
                foreach (string serviceName in importantServices)
                {
                    using var sc = new ServiceController(serviceName);
                    string status = sc.Status.ToString();
                    string displayName = sc.DisplayName;
                    
                    serviceStatus += $"{displayName}: {status}\n";
                }
                
                return serviceStatus;
            }
            catch (Exception ex)
            {
                return $"サービス情報を取得できませんでした: {ex.Message}";
            }
        }

        public void Dispose()
        {
            cpuCounter?.Dispose();
            ramCounter?.Dispose();
        }
    }
}
