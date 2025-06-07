using System.Management;

namespace AimAssist.Units.Implementation.Computer.Services
{
    public class DriverInfoService
    {
        public string GetDriverVersions()
        {
            string driverVersions = string.Empty;
            
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver");
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    string? deviceName = obj["DeviceName"]?.ToString();
                    string? driverVersion = obj["DriverVersion"]?.ToString();
                    string? manufacturer = obj["Manufacturer"]?.ToString();
                    
                    if (!string.IsNullOrEmpty(deviceName) && !string.IsNullOrEmpty(driverVersion))
                    {
                        driverVersions += $"デバイス: {deviceName}\n";
                        driverVersions += $"メーカー: {manufacturer ?? "不明"}\n";
                        driverVersions += $"ドライババージョン: {driverVersion}\n\n";
                    }
                }
                
                if (string.IsNullOrEmpty(driverVersions))
                {
                    driverVersions = "ドライバー情報が見つかりませんでした。";
                }
            }
            catch
            {
                driverVersions = "ドライバー情報を取得できませんでした";
            }
            
            return driverVersions;
        }

        public string GetProblemDevices()
        {
            string problemDevices = string.Empty;
            
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode <> 0");
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    string? deviceName = obj["Name"]?.ToString();
                    string? deviceId = obj["DeviceID"]?.ToString();
                    int errorCode = Convert.ToInt32(obj["ConfigManagerErrorCode"]);
                    
                    problemDevices += $"デバイス: {deviceName ?? "不明"}\n";
                    problemDevices += $"デバイスID: {deviceId ?? "不明"}\n";
                    problemDevices += $"エラーコード: {errorCode}\n\n";
                }
                
                if (string.IsNullOrEmpty(problemDevices))
                {
                    problemDevices = "問題のあるデバイスは見つかりませんでした。";
                }
            }
            catch
            {
                problemDevices = "デバイス状態情報を取得できませんでした";
            }
            
            return problemDevices;
        }
    }
}
