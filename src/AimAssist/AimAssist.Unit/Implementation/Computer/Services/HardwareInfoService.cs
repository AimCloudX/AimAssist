using System.Management;

namespace AimAssist.Units.Implementation.Computer.Services
{
    public class HardwareInfoService
    {
        public string GetOsVersionInfo()
        {
            string osInfo = "Unknown";
            
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem");
                foreach (ManagementObject os in searcher.Get().Cast<ManagementObject>())
                {
                    osInfo = $"{os["Caption"]} {os["Version"]}";
                }
            }
            catch
            {
                osInfo = Environment.OSVersion.VersionString;
            }
            
            return osInfo;
        }

        public string GetCpuInfo()
        {
            string cpuInfo = string.Empty;
            
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, MaxClockSpeed FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    string name = obj["Name"]?.ToString() ?? "";
                    string cores = obj["NumberOfCores"]?.ToString() ?? "";
                    string speed = obj["MaxClockSpeed"]?.ToString() ?? "";
                    
                    cpuInfo = $"{name}, {cores}コア, {Convert.ToDouble(speed) / 1000:F2} GHz";
                }
            }
            catch
            {
                cpuInfo = "情報を取得できませんでした";
            }
            
            return cpuInfo;
        }

        public string GetTotalPhysicalMemory()
        {
            string memoryInfo = string.Empty;
            
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem");
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    double totalMemory = Convert.ToDouble(obj["TotalPhysicalMemory"]);
                    double totalMemoryGb = totalMemory / (1024 * 1024 * 1024);
                    memoryInfo = $"{totalMemoryGb:F2} GB";
                }
            }
            catch
            {
                memoryInfo = "情報を取得できませんでした";
            }
            
            return memoryInfo;
        }

        public string GetGpuInfo()
        {
            string gpuInfo = string.Empty;
            
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM FROM Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    string name = obj["Name"]?.ToString() ?? "";
                    
                    if (obj["AdapterRAM"] != null)
                    {
                        ulong ram = Convert.ToUInt64(obj["AdapterRAM"]);
                        double ramGb = ram / (1024.0 * 1024 * 1024);
                        gpuInfo += $"{name}, {ramGb:F2} GB\n";
                    }
                    else
                    {
                        gpuInfo += $"{name}\n";
                    }
                }
            }
            catch
            {
                gpuInfo = "情報を取得できませんでした";
            }
            
            return gpuInfo;
        }

        public string GetMotherboardInfo()
        {
            string motherboardInfo = string.Empty;
            
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard");
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    string manufacturer = obj["Manufacturer"]?.ToString() ?? "";
                    string product = obj["Product"]?.ToString() ?? "";
                    
                    motherboardInfo = $"{manufacturer} {product}";
                }
            }
            catch
            {
                motherboardInfo = "情報を取得できませんでした";
            }
            
            return motherboardInfo;
        }
    }
}
