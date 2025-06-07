using System.Diagnostics;
using System.Management;

namespace AimAssist.Units.Implementation.Computer.Services
{
    public class ResourceUsageService
    {
        public string GetProcessInfo()
        {
            try
            {
                Process[] processes = Process.GetProcesses();
                var topCpuProcesses = processes
                    .OrderByDescending(p =>
                    {
                        try
                        {
                            return p.TotalProcessorTime.TotalMilliseconds;
                        }
                        catch
                        {
                            return 0;
                        }
                    })
                    .Take(5);
                
                string processInfo = "上位CPUプロセス:\n";
                
                foreach (var process in topCpuProcesses)
                {
                    try
                    {
                        processInfo += $"{process.ProcessName}: {process.TotalProcessorTime.TotalSeconds:F1}秒, スレッド数: {process.Threads.Count}\n";
                    }
                    catch
                    {
                        // プロセス情報を取得できない場合はスキップ
                    }
                }
                
                var topMemoryProcesses = processes
                    .OrderByDescending(p =>
                    {
                        try
                        {
                            return p.WorkingSet64;
                        }
                        catch
                        {
                            return 0;
                        }
                    })
                    .Take(5);
                
                processInfo += "\n上位メモリプロセス:\n";
                
                foreach (var process in topMemoryProcesses)
                {
                    try
                    {
                        double memoryMb = process.WorkingSet64 / (1024.0 * 1024);
                        processInfo += $"{process.ProcessName}: {memoryMb:F1} MB, スレッド数: {process.Threads.Count}\n";
                    }
                    catch
                    {
                        // プロセス情報を取得できない場合はスキップ
                    }
                }
                
                return processInfo;
            }
            catch (Exception ex)
            {
                return $"プロセス情報を取得できませんでした: {ex.Message}";
            }
        }

        public string GetHeapMemoryInfo()
        {
            string heapInfo = string.Empty;
            
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    ulong totalVirtualMemory = Convert.ToUInt64(obj["TotalVirtualMemorySize"]) * 1024;
                    ulong freeVirtualMemory = Convert.ToUInt64(obj["FreeVirtualMemory"]) * 1024;
                    ulong totalVisibleMemory = Convert.ToUInt64(obj["TotalVisibleMemorySize"]) * 1024;
                    ulong freePhysicalMemory = Convert.ToUInt64(obj["FreePhysicalMemory"]) * 1024;
                    
                    double totalVirtualGb = totalVirtualMemory / (1024.0 * 1024 * 1024);
                    double freeVirtualGb = freeVirtualMemory / (1024.0 * 1024 * 1024);
                    double totalVisibleGb = totalVisibleMemory / (1024.0 * 1024 * 1024);
                    double freePhysicalGb = freePhysicalMemory / (1024.0 * 1024 * 1024);
                    
                    heapInfo += $"総仮想メモリ: {totalVirtualGb:F2} GB\n";
                    heapInfo += $"空き仮想メモリ: {freeVirtualGb:F2} GB\n";
                    heapInfo += $"使用中仮想メモリ: {(totalVirtualGb - freeVirtualGb):F2} GB\n\n";
                    heapInfo += $"総物理メモリ: {totalVisibleGb:F2} GB\n";
                    heapInfo += $"空き物理メモリ: {freePhysicalGb:F2} GB\n";
                    heapInfo += $"使用中物理メモリ: {(totalVisibleGb - freePhysicalGb):F2} GB\n";
                }
            }
            catch
            {
                heapInfo = "ヒープメモリ情報を取得できませんでした";
            }
            
            return heapInfo;
        }
    }
}
