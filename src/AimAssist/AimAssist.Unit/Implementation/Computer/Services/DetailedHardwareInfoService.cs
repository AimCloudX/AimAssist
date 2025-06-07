using System.Management;

namespace AimAssist.Units.Implementation.Computer.Services
{
    public class DetailedHardwareInfoService
    {
        public string GetCpuTemperature()
        {
            string temperature = "情報を取得できませんでした";
            
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    double tempKelvin = Convert.ToDouble(obj["CurrentTemperature"]?.ToString() ?? "0");
                    double tempCelsius = tempKelvin / 10.0 - 273.15;
                    
                    temperature = $"{tempCelsius:F1} °C";
                    break;
                }
            }
            catch
            {
                // Open Hardware Monitorのようなサードパーティツールの使用が必要かもしれません
            }
            
            return temperature;
        }

        public string GetGpuTemperature()
        {
            string temperature = "情報を取得できませんでした";
            
            try
            {
                // NVIDIAのGPUの場合
                // この機能を実装するには、NVIDIAのNVAPIやAMDのADL SDKを使用する必要があります
                // ここでは、サンプルコードとしてプレースホルダーを使用します
            }
            catch
            {
                // 何もしない
            }
            
            return temperature;
        }

        public string GetStorageDetails()
        {
            string storageDetails = string.Empty;
            
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject disk in searcher.Get().Cast<ManagementObject>())
                {
                    string model = disk["Model"]?.ToString() ?? "";
                    string interfaceType = disk["InterfaceType"]?.ToString() ?? "";
                    string mediaType = disk["MediaType"]?.ToString() ?? "不明";
                    ulong size = Convert.ToUInt64(disk["Size"]);
                    double sizeGb = size / (1024.0 * 1024 * 1024);
                    
                    storageDetails += $"モデル: {model}\n";
                    storageDetails += $"インターフェース: {interfaceType}\n";
                    storageDetails += $"メディアタイプ: {mediaType}\n";
                    storageDetails += $"容量: {sizeGb:F2} GB\n\n";
                }
            }
            catch
            {
                storageDetails = "情報を取得できませんでした";
            }
            
            return storageDetails;
        }

        public string GetBatteryStatus()
        {
            string batteryStatus = "バッテリー情報がありません（デスクトップPCまたは情報を取得できません）";

            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
                foreach (ManagementObject battery in searcher.Get().Cast<ManagementObject>())
                {
                    int estimatedChargeRemaining = Convert.ToInt32(battery["EstimatedChargeRemaining"]);
                    string statusValue = battery["BatteryStatus"]?.ToString() ?? "";
                    
                    string status = statusValue switch
                    {
                        "1" => "ディスチャージ中",
                        "2" => "AC電源から充電中",
                        "3" => "完全充電",
                        "4" => "低",
                        "5" => "危険",
                        _ => "不明"
                    };
                    
                    batteryStatus = $"バッテリー残量: {estimatedChargeRemaining}%\n";
                    batteryStatus += $"状態: {status}\n";
                }
            }
            catch
            {
                // 何もしない
            }
            
            return batteryStatus;
        }
    }
}
