using System.Management;

namespace AimAssist.Units.Implementation.Computer.Services
{
    public class SecurityInfoService
    {
        public string GetWindowsDefenderStatus()
        {
            string defenderStatus = string.Empty;
            
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\Microsoft\Windows\Defender", "SELECT * FROM MSFT_MpComputerStatus");
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    bool realTimeProtectionEnabled = Convert.ToBoolean(obj["RealTimeProtectionEnabled"]);
                    string avSignatureVersion = obj["AntivirusSignatureVersion"]?.ToString() ?? "";
                    DateTime lastScanTime = ManagementDateTimeConverter.ToDateTime(obj["FullScanEndTime"]?.ToString() ?? "");
                    
                    defenderStatus = $"リアルタイム保護: {(realTimeProtectionEnabled ? "有効" : "無効")}\n";
                    defenderStatus += $"ウイルス定義バージョン: {avSignatureVersion}\n";
                    defenderStatus += $"最終スキャン: {lastScanTime}";
                }
            }
            catch
            {
                defenderStatus = "情報を取得できませんでした";
            }
            
            return defenderStatus;
        }

        public string GetFirewallStatus()
        {
            string firewallStatus = string.Empty;
            
            try
            {
                using var searcher = new ManagementObjectSearcher(@"root\StandardCimv2", "SELECT * FROM MSFT_NetFirewallProfile");
                foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
                {
                    string profileName = obj["Name"]?.ToString() ?? "";
                    bool enabled = Convert.ToBoolean(obj["Enabled"]);
                    
                    firewallStatus += $"{profileName} プロファイル: {(enabled ? "有効" : "無効")}\n";
                }
            }
            catch
            {
                firewallStatus = "情報を取得できませんでした";
            }
            
            return firewallStatus;
        }

        public string GetSecurityUpdateStatus()
        {
            string updateStatus = string.Empty;
            
            try
            {
                using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_QuickFixEngineering ORDER BY InstalledOn DESC");
                ManagementObjectCollection collection = searcher.Get();
                
                if (collection.Count > 0)
                {
                    ManagementObject? latestUpdate = collection.Cast<ManagementObject>().FirstOrDefault();
                    
                    if (latestUpdate != null)
                    {
                        string updateId = latestUpdate["HotFixID"]?.ToString() ?? "";
                        string description = latestUpdate["Description"]?.ToString() ?? "不明";
                        string installedOn = latestUpdate["InstalledOn"]?.ToString() ?? "不明";
                        
                        updateStatus = $"最新のセキュリティパッチ: {updateId}\n";
                        updateStatus += $"種類: {description}\n";
                        updateStatus += $"インストール日: {installedOn}\n";
                        updateStatus += $"合計更新プログラム数: {collection.Count}";
                    }
                }
                else
                {
                    updateStatus = "セキュリティ更新プログラムが見つかりませんでした。";
                }
            }
            catch
            {
                updateStatus = "情報を取得できませんでした";
            }
            
            return updateStatus;
        }
    }
}
