using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AimAssist.Units.Implementation.Computer
{
    /// <summary>
    /// ComputerView.xaml の相互作用ロジック
    /// </summary>
    public partial class ComputerView : UserControl
    {
        private DispatcherTimer _timer;
        private PerformanceCounter _cpuCounter;
        private PerformanceCounter _ramCounter;
        public ComputerView()
        {
            InitializeComponent();

            this.DataContext = this;

            LoadData();
            
            // リアルタイム更新のためのタイマーを設定
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private async void IntializeWebView(string loc)
        {
            await LocationWebView.EnsureCoreWebView2Async(null);
            var url = $"https://www.google.com/maps?q={loc}";
            LocationWebView.Source = new Uri(url);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // リアルタイムで更新する必要がある情報を更新
            Task.Run(() =>
            {
                UpdatePerformanceInfo();
                UpdateNetworkStatus();
            });
        }

        private void LoadData()
        {
            // 各タブの情報を読み込む
            Task.Run(() =>
            {
                // CPUとRAMの使用率を監視するためのPerformanceCounterを初期化
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _ramCounter = new PerformanceCounter("Memory", "% Committed Bytes In Use");

                LoadBasicInfo();
                LoadHardwareInfo();
                LoadNetworkInfo();
                LoadPerformanceInfo();
                LoadSecurityInfo();
                LoadDetailedHardwareInfo();
                //LoadSystemLogsInfo();
                //LoadSoftwareInfo();
                //LoadAdvancedNetworkInfo();
                //LoadAdvancedSecurityInfo();
                LoadResourceUsageInfo();
                //LoadBackupInfo();
                LoadDriverInfo();
            });
        }

        #region 基本情報
        private void LoadBasicInfo()
        {
            try
            {
                // OSのバージョン
                string osVersion = GetOSVersionInfo();

                // システムアーキテクチャ
                string architecture = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
                // 起動時間
                TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);

                Dispatcher.Invoke(() => {
                    // コンピューター名
                    txtComputerName.Text = Environment.MachineName;
                    txtOSVersion.Text = osVersion;
                    txtArchitecture.Text = architecture;
                    // ユーザー名
                    txtUsername.Text = Environment.UserName;
                    // ホスト名
                    txtHostname.Text = Dns.GetHostName();
                    // ドメイン名
                    txtDomain.Text = Environment.UserDomainName;

                    txtUptime.Text = $"{uptime.Days}日 {uptime.Hours}時間 {uptime.Minutes}分 {uptime.Seconds}秒";
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"基本情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetOSVersionInfo()
        {
            string osInfo = "Unknown";
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject os in searcher.Get())
                    {
                        osInfo = os["Caption"].ToString() + " " + os["Version"].ToString();
                    }
                }
            }
            catch
            {
                // フォールバック方法
                osInfo = Environment.OSVersion.VersionString;
            }
            
            return osInfo;
        }
        #endregion

        #region ハードウェア情報
        private void LoadHardwareInfo()
        {
            try
            {
                // CPU情報
                var aa  = GetCPUInfo();
                // メモリ情報
                var bb = GetTotalPhysicalMemory();
                // ディスク情報
                var cc  = GetDiskInfo();
                // GPU情報
                var dd = GetGPUInfo();
                // マザーボード情報
                var ee = GetMotherboardInfo();
                Dispatcher.Invoke(() => {
                    txtCPUInfo.Text = aa;
                    txtMemoryInfo.Text = bb;
                    txtDiskInfo.Text = cc;
                    txtGPUInfo.Text = dd;
                    txtMotherboardInfo.Text = ee;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ハードウェア情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetCPUInfo()
        {
            string cpuInfo = string.Empty;
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, NumberOfCores, MaxClockSpeed FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"].ToString();
                        string cores = obj["NumberOfCores"].ToString();
                        string speed = obj["MaxClockSpeed"].ToString();
                        
                        cpuInfo = $"{name}, {cores}コア, {Convert.ToDouble(speed) / 1000:F2} GHz";
                    }
                }
            }
            catch
            {
                cpuInfo = "情報を取得できませんでした";
            }
            
            return cpuInfo;
        }

        private string GetTotalPhysicalMemory()
        {
            string memoryInfo = string.Empty;
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT TotalPhysicalMemory FROM Win32_ComputerSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        double totalMemory = Convert.ToDouble(obj["TotalPhysicalMemory"]);
                        double totalMemoryGB = totalMemory / (1024 * 1024 * 1024);
                        memoryInfo = $"{totalMemoryGB:F2} GB";
                    }
                }
            }
            catch
            {
                memoryInfo = "情報を取得できませんでした";
            }
            
            return memoryInfo;
        }

        private string GetDiskInfo()
        {
            string diskInfo = string.Empty;
            
            try
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                
                foreach (DriveInfo drive in allDrives)
                {
                    if (drive.IsReady)
                    {
                        double totalSizeGB = drive.TotalSize / (1024.0 * 1024 * 1024);
                        double freeSpaceGB = drive.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                        double usedSpaceGB = totalSizeGB - freeSpaceGB;
                        
                        diskInfo += $"ドライブ {drive.Name} ({drive.DriveType}): 総容量 {totalSizeGB:F2} GB, 使用済み {usedSpaceGB:F2} GB, 空き {freeSpaceGB:F2} GB\n";
                    }
                }
            }
            catch
            {
                diskInfo = "情報を取得できませんでした";
            }
            
            return diskInfo;
        }

        private string GetGPUInfo()
        {
            string gpuInfo = string.Empty;
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string name = obj["Name"].ToString();
                        
                        if (obj["AdapterRAM"] != null)
                        {
                            ulong ram = Convert.ToUInt64(obj["AdapterRAM"]);
                            double ramGB = ram / (1024.0 * 1024 * 1024);
                            gpuInfo += $"{name}, {ramGB:F2} GB\n";
                        }
                        else
                        {
                            gpuInfo += $"{name}\n";
                        }
                    }
                }
            }
            catch
            {
                gpuInfo = "情報を取得できませんでした";
            }
            
            return gpuInfo;
        }

        private string GetMotherboardInfo()
        {
            string motherboardInfo = string.Empty;
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT Manufacturer, Product FROM Win32_BaseBoard"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string manufacturer = obj["Manufacturer"].ToString();
                        string product = obj["Product"].ToString();
                        
                        motherboardInfo = $"{manufacturer} {product}";
                    }
                }
            }
            catch
            {
                motherboardInfo = "情報を取得できませんでした";
            }
            
            return motherboardInfo;
        }
        #endregion

        #region ネットワーク情報
        private void LoadNetworkInfo()
        {
            try
            {
                // IPアドレス情報
                Dictionary<string, string> ipAddresses = GetIPAddresses();
                // MACアドレス
                var aa = GetMACAddress();
                Dispatcher.Invoke(() => {
                    txtIPv4Address.Text = ipAddresses.ContainsKey("IPv4") ? ipAddresses["IPv4"] : "取得できませんでした";
                    txtIPv6Address.Text = ipAddresses.ContainsKey("IPv6") ? ipAddresses["IPv6"] : "取得できませんでした";
                    txtMACAddress.Text = aa;
                });
                
                // ネットワーク接続状態
                UpdateNetworkStatus();
                
                // 外部IPアドレス
                LoadIPInfoAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ネットワーク情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private Dictionary<string, string> GetIPAddresses()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            
            try
            {
                IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
                
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        result["IPv4"] = ip.ToString();
                    }
                    else if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        result["IPv6"] = ip.ToString();
                    }
                }
            }
            catch
            {
                // 何もしない
            }
            
            return result;
        }

        private string GetMACAddress()
        {
            string macAddress = string.Empty;
            
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                
                foreach (NetworkInterface adapter in nics)
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up && 
                        (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet || 
                         adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                    {
                        PhysicalAddress address = adapter.GetPhysicalAddress();
                        byte[] bytes = address.GetAddressBytes();
                        
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            macAddress += bytes[i].ToString("X2");
                            
                            if (i != bytes.Length - 1)
                            {
                                macAddress += "-";
                            }
                        }
                        
                        break;
                    }
                }
            }
            catch
            {
                macAddress = "取得できませんでした";
            }
            
            return macAddress;
        }

        private void UpdateNetworkStatus()
        {
            try
            {
                NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
                string connectionInfo = "";
                
                foreach (NetworkInterface adapter in nics)
                {
                    if (adapter.OperationalStatus == OperationalStatus.Up && 
                        (adapter.NetworkInterfaceType == NetworkInterfaceType.Ethernet || 
                         adapter.NetworkInterfaceType == NetworkInterfaceType.Wireless80211))
                    {
                        string speed = (adapter.Speed / 1000000).ToString() + " Mbps";
                        string type = adapter.NetworkInterfaceType.ToString();
                        
                        connectionInfo += $"{adapter.Name} ({type}), 速度: {speed}, 状態: {adapter.OperationalStatus}\n";
                    }
                }
                
                Dispatcher.Invoke(() => {
                txtNetworkStatus.Text = connectionInfo;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => {
                txtNetworkStatus.Text = $"接続情報を取得できませんでした: {ex.Message}";
                });
            }
        }

        #endregion

        #region パフォーマンス情報
        private void LoadPerformanceInfo()
        {
            try
            {
                // 現在のCPU使用率、メモリ使用率、ディスク使用率を取得
                UpdatePerformanceInfo();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"パフォーマンス情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdatePerformanceInfo()
        {
            try
            {
                // CPU使用率
                if(_cpuCounter != null)
                {
                    float cpuUsage = _cpuCounter.NextValue();
                    Dispatcher.Invoke(() =>
                    {
                        txtCPUUsage.Text = $"{cpuUsage:F1}%";
                    });

                }

                if(_ramCounter != null)
                {
                    // メモリ使用率
                    float ramUsage = _ramCounter.NextValue();
                    Dispatcher.Invoke(() =>
                    {
                        txtMemoryUsage.Text = $"{ramUsage:F1}%";
                    });
                }

                // ディスク使用率
                string diskUsage = GetDiskUsage();
                Dispatcher.Invoke(() => {
                txtDiskUsage.Text = diskUsage;
                });
                
                // サービスの状態
                UpdateServiceStatus();
                
                // 起動時間の更新
                TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
                Dispatcher.Invoke(() => {
                txtUptime.Text = $"{uptime.Days}日 {uptime.Hours}時間 {uptime.Minutes}分 {uptime.Seconds}秒";
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"パフォーマンス情報の更新中にエラーが発生しました: {ex.Message}");
            }
        }

        private string GetDiskUsage()
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

        private void UpdateServiceStatus()
        {
            try
            {
                string serviceStatus = string.Empty;
                
                // 重要なサービスの状態をチェック
                string[] importantServices = { "Dhcp", "DNSCache", "BITS", "wuauserv" }; // DHCPクライアント, DNSキャッシュ, BITSサービス, Windows Update
                
                foreach (string serviceName in importantServices)
                {
                    using (ServiceController sc = new ServiceController(serviceName))
                    {
                        string status = sc.Status.ToString();
                        string displayName = sc.DisplayName;
                        
                        serviceStatus += $"{displayName}: {status}\n";
                    }
                }
                
                Dispatcher.Invoke(() => {
                txtServiceStatus.Text = serviceStatus;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => {
                txtServiceStatus.Text = $"サービス情報を取得できませんでした: {ex.Message}";
                });
            }
        }
        #endregion

        #region セキュリティ情報
        private void LoadSecurityInfo()
        {
            try
            {
                // Windows Defenderの状態
                var aa  = GetWindowsDefenderStatus();
                Dispatcher.Invoke(() => {
                    txtDefenderStatus.Text = aa;
                });
                
                // ファイアウォールの状態
                var bb  = GetFirewallStatus();
                Dispatcher.Invoke(() => {
                    txtFirewallStatus.Text = bb;
                });
                
                // セキュリティ更新の状態
                var cc = GetSecurityUpdateStatus();
                Dispatcher.Invoke(() => {
                    txtSecurityUpdates.Text = cc;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"セキュリティ情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetWindowsDefenderStatus()
        {
            string defenderStatus = string.Empty;
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\Microsoft\Windows\Defender", "SELECT * FROM MSFT_MpComputerStatus"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        bool realTimeProtectionEnabled = Convert.ToBoolean(obj["RealTimeProtectionEnabled"]);
                        string avSignatureVersion = obj["AntivirusSignatureVersion"].ToString();
                        DateTime lastScanTime = ManagementDateTimeConverter.ToDateTime(obj["FullScanEndTime"].ToString());
                        
                        defenderStatus = $"リアルタイム保護: {(realTimeProtectionEnabled ? "有効" : "無効")}\n";
                        defenderStatus += $"ウイルス定義バージョン: {avSignatureVersion}\n";
                        defenderStatus += $"最終スキャン: {lastScanTime}";
                    }
                }
            }
            catch
            {
                defenderStatus = "情報を取得できませんでした";
            }
            
            return defenderStatus;
        }

        private string GetFirewallStatus()
        {
            string firewallStatus = string.Empty;
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\StandardCimv2", "SELECT * FROM MSFT_NetFirewallProfile"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string profileName = obj["Name"].ToString();
                        bool enabled = Convert.ToBoolean(obj["Enabled"]);
                        
                        firewallStatus += $"{profileName} プロファイル: {(enabled ? "有効" : "無効")}\n";
                    }
                }
            }
            catch
            {
                firewallStatus = "情報を取得できませんでした";
            }
            
            return firewallStatus;
        }

        private string GetSecurityUpdateStatus()
        {
            string updateStatus = string.Empty;
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_QuickFixEngineering ORDER BY InstalledOn DESC"))
                {
                    ManagementObjectCollection collection = searcher.Get();
                    
                    if (collection.Count > 0)
                    {
                        ManagementObject latestUpdate = collection.Cast<ManagementObject>().FirstOrDefault();
                        
                        if (latestUpdate != null)
                        {
                            string updateId = latestUpdate["HotFixID"].ToString();
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
            }
            catch
            {
                updateStatus = "情報を取得できませんでした";
            }
            
            return updateStatus;
        }
        #endregion

        #region 詳細なハードウェア情報
        private void LoadDetailedHardwareInfo()
        {
            try
            {
                // CPU温度
                var aa = GetCPUTemperature();
                Dispatcher.Invoke(() => {
                txtCPUTemperature.Text =aa;
                });
                
                // GPU温度
                var bb  = GetGPUTemperature();
                Dispatcher.Invoke(() => {
                    txtGPUTemperature.Text = bb;
                });

                // ストレージの詳細情報
                var cc =GetStorageDetails();
                Dispatcher.Invoke(() => {
                    txtStorageDetails.Text = cc;
                });
                
                // バッテリー情報
                var dd  = GetBatteryStatus();
                Dispatcher.Invoke(() => {
                    txtBatteryStatus.Text = dd;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"詳細なハードウェア情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetCPUTemperature()
        {
            string temperature = "情報を取得できませんでした";
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        // 温度はケルビン単位で取得されるため、摂氏に変換
                        double tempKelvin = Convert.ToDouble(obj["CurrentTemperature"].ToString());
                        double tempCelsius = tempKelvin / 10.0 - 273.15;
                        
                        temperature = $"{tempCelsius:F1} °C";
                        break;
                    }
                }
            }
            catch
            {
                // Open Hardware Monitorのようなサードパーティツールの使用が必要かもしれません
            }
            
            return temperature;
        }

        private string GetGPUTemperature()
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

        private string GetStorageDetails()
        {
            string storageDetails = string.Empty;
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
                {
                    foreach (ManagementObject disk in searcher.Get())
                    {
                        string model = disk["Model"].ToString();
                        string interfaceType = disk["InterfaceType"].ToString();
                        string mediaType = disk["MediaType"]?.ToString() ?? "不明";
                        ulong size = Convert.ToUInt64(disk["Size"]);
                        double sizeGB = size / (1024.0 * 1024 * 1024);
                        
                        storageDetails += $"モデル: {model}\n";
                        storageDetails += $"インターフェース: {interfaceType}\n";
                        storageDetails += $"メディアタイプ: {mediaType}\n";
                        storageDetails += $"容量: {sizeGB:F2} GB\n\n";
                    }
                }
            }
            catch
            {
                storageDetails = "情報を取得できませんでした";
            }
            
            return storageDetails;
        }

        private string GetBatteryStatus()
        {
            string batteryStatus = "バッテリー情報がありません（デスクトップPCまたは情報を取得できません）";

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery"))
                {
                    foreach (ManagementObject battery in searcher.Get())
                    {
                        int estimatedChargeRemaining = Convert.ToInt32(battery["EstimatedChargeRemaining"]);
                        batteryStatus = battery["BatteryStatus"].ToString();
                        
                        string status;
                        switch (batteryStatus)
                        {
                            case "1":
                                status = "ディスチャージ中";
                                break;
                            case "2":
                                status = "AC電源から充電中";
                                break;
                            case "3":
                                status = "完全充電";
                                break;
                            case "4":
                                status = "低";
                                break;
                            case "5":
                                status = "危険";
                                break;
                            default:
                                status = "不明";
                                break;
                        }
                        
                        batteryStatus = $"バッテリー残量: {estimatedChargeRemaining}%\n";
                        batteryStatus += $"状態: {status}\n";
                    }
                }
            }
            catch
            {
                // 何もしない
            }
            
            return batteryStatus;
        }
        #endregion

        #region システムログとエラー情報
        //private void LoadSystemLogsInfo()
        //{
        //    try
        //    {
        //        // システムイベントログ
        //        txtSystemEventLogs.Text = GetSystemEventLogs();
                
        //        // アプリケーションクラッシュレポート
        //        txtApplicationCrashReports.Text = GetApplicationCrashReports();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"システムログ情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private string GetSystemEventLogs()
        {
            string eventLogs = string.Empty;
            
            try
            {
                // システムログから最新のエラーと警告を取得
                EventLog systemLog = new EventLog("System");
                int count = 0;
                
                foreach (EventLogEntry entry in systemLog.Entries)
                {
                    if ((entry.EntryType == EventLogEntryType.Error || entry.EntryType == EventLogEntryType.Warning) 
                        && count < 10)
                    {
                        eventLogs += $"日時: {entry.TimeGenerated}\n";
                        eventLogs += $"種類: {entry.EntryType}\n";
                        eventLogs += $"ソース: {entry.Source}\n";
                        eventLogs += $"ID: {entry.InstanceId}\n";
                        eventLogs += $"メッセージ: {entry.Message}\n\n";
                        
                        count++;
                    }
                }
                
                if (count == 0)
                {
                    eventLogs = "最近のシステムエラーや警告はありません。";
                }
            }
            catch
            {
                eventLogs = "システムログ情報を取得できませんでした";
            }
            
            return eventLogs;
        }

        private string GetApplicationCrashReports()
        {
            string crashReports = string.Empty;
            
            try
            {
                // アプリケーションログからクラッシュレポートを取得
                EventLog applicationLog = new EventLog("Application");
                int count = 0;
                
                foreach (EventLogEntry entry in applicationLog.Entries)
                {
                    if (entry.EntryType == EventLogEntryType.Error && count < 10)
                    {
                        crashReports += $"日時: {entry.TimeGenerated}\n";
                        crashReports += $"アプリケーション: {entry.Source}\n";
                        crashReports += $"ID: {entry.InstanceId}\n";
                        crashReports += $"メッセージ: {entry.Message}\n\n";
                        
                        count++;
                    }
                }
                
                if (count == 0)
                {
                    crashReports = "最近のアプリケーションクラッシュは記録されていません。";
                }
            }
            catch
            {
                crashReports = "アプリケーションクラッシュ情報を取得できませんでした";
            }
            
            return crashReports;
        }
        #endregion

        #region ソフトウェア・ライセンス情報
        //private void LoadSoftwareInfo()
        //{
        //    try
        //    {
        //        // インストールされているソフトウェアの一覧
        //        txtInstalledSoftware.Text = GetInstalledSoftware();

        //        // Windowsライセンスの状態
        //        txtWindowsLicense.Text = GetWindowsLicenseStatus();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"ソフトウェア情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private string GetInstalledSoftware()
        {
            string installedSoftware = string.Empty;
            
            try
            {
                // レジストリからインストールされているソフトウェア情報を取得
                string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
                
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
                {
                    if (key != null)
                    {
                        foreach (string subKeyName in key.GetSubKeyNames())
                        {
                            using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                            {
                                if (subKey != null)
                                {
                                    string displayName = subKey.GetValue("DisplayName") as string;
                                    string displayVersion = subKey.GetValue("DisplayVersion") as string;
                                    
                                    if (!string.IsNullOrEmpty(displayName))
                                    {
                                        installedSoftware += $"{displayName} - {displayVersion ?? "バージョン不明"}\n";
                                    }
                                }
                            }
                        }
                    }
                }
                
                // 64ビット環境の場合、32ビットアプリケーションも取得
                registryKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey))
                {
                    if (key != null)
                    {
                        foreach (string subKeyName in key.GetSubKeyNames())
                        {
                            using (RegistryKey subKey = key.OpenSubKey(subKeyName))
                            {
                                if (subKey != null)
                                {
                                    string displayName = subKey.GetValue("DisplayName") as string;
                                    string displayVersion = subKey.GetValue("DisplayVersion") as string;
                                    
                                    if (!string.IsNullOrEmpty(displayName))
                                    {
                                        installedSoftware += $"{displayName} - {displayVersion ?? "バージョン不明"}\n";
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                installedSoftware = "ソフトウェア情報を取得できませんでした";
            }
            
            return installedSoftware;
        }

        private string GetWindowsLicenseStatus()
        {
            string licenseStatus = string.Empty;
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM SoftwareLicensingProduct WHERE Name LIKE 'Windows%'"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        int licenseStatusValue = Convert.ToInt32(obj["LicenseStatus"]);
                        string licenseStatusText;
                        
                        switch (licenseStatusValue)
                        {
                            case 0:
                                licenseStatusText = "ライセンスなし";
                                break;
                            case 1:
                                licenseStatusText = "ライセンス済み";
                                break;
                            case 2:
                                licenseStatusText = "初期猶予期間";
                                break;
                            case 3:
                                licenseStatusText = "追加猶予期間";
                                break;
                            case 4:
                                licenseStatusText = "制限付きモード";
                                break;
                            default:
                                licenseStatusText = "不明";
                                break;
                        }
                        
                        string productName = obj["Name"].ToString();
                        string productKeyChannel = obj["ProductKeyChannel"]?.ToString() ?? "不明";
                        
                        licenseStatus += $"製品名: {productName}\n";
                        licenseStatus += $"ライセンス状態: {licenseStatusText}\n";
                        licenseStatus += $"プロダクトキーチャネル: {productKeyChannel}\n";
                    }
                }
            }
            catch
            {
                licenseStatus = "ライセンス情報を取得できませんでした";
            }
            
            return licenseStatus;
        }
        #endregion

        #region 高度なネットワーク情報
        //private void LoadAdvancedNetworkInfo()
        //{
        //    try
        //    {
        //        // ポート状態
        //        txtOpenPorts.Text = GetOpenPorts();
                
        //        // 接続されているデバイス
        //        txtConnectedDevices.Text = GetConnectedDevices();
                
        //        // ネットワーク接続の遅延（Ping）
        //        GetNetworkLatency();
                
        //        // ネットワーク帯域幅の使用量
        //        txtNetworkBandwidth.Text = GetNetworkBandwidth();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"高度なネットワーク情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private string GetOpenPorts()
        {
            string openPorts = string.Empty;
            
            try
            {
                // アクティブなTCP接続を取得
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "netstat";
                    process.StartInfo.Arguments = "-ano";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    
                    process.Start();
                    
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    
                    // 出力を解析
                    string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (string line in lines)
                    {
                        if (line.Contains("LISTENING"))
                        {
                            openPorts += line + "\n";
                        }
                    }
                }
            }
            catch
            {
                openPorts = "ポート情報を取得できませんでした";
            }
            
            return openPorts;
        }

        private string GetConnectedDevices()
        {
            string connectedDevices = string.Empty;
            
            try
            {
                // ARPテーブルを使ってネットワーク上のデバイスを検出
                using (Process process = new Process())
                {
                    process.StartInfo.FileName = "arp";
                    process.StartInfo.Arguments = "-a";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.CreateNoWindow = true;
                    
                    process.Start();
                    
                    string output = process.StandardOutput.ReadToEnd();
                    process.WaitForExit();
                    
                    connectedDevices = output;
                }
            }
            catch
            {
                connectedDevices = "接続デバイス情報を取得できませんでした";
            }
            
            return connectedDevices;
        }

        //private async void GetNetworkLatency()
        //{
        //    try
        //    {
        //        txtNetworkLatency.Text = "測定中...";
                
        //        await Task.Run(() =>
        //        {
        //            try
        //            {
        //                Ping ping = new Ping();
        //                string[] hostsToPing = { "8.8.8.8", "1.1.1.1", "www.google.com" }; // Googleの公開DNSサーバー、Cloudflare DNS、Googleのウェブサイト
                        
        //                string result = "";
                        
        //                foreach (string host in hostsToPing)
        //                {
        //                    try
        //                    {
        //                        PingReply reply = ping.Send(host);
                                
        //                        if (reply.Status == IPStatus.Success)
        //                        {
        //                            result += $"{host}: {reply.RoundtripTime}ms\n";
        //                        }
        //                        else
        //                        {
        //                            result += $"{host}: {reply.Status}\n";
        //                        }
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        result += $"{host}: エラー - {ex.Message}\n";
        //                    }
        //                }
                        
        //                // UIスレッドで更新
        //                Dispatcher.Invoke(() =>
        //                {
        //                    txtNetworkLatency.Text = result;
        //                });
        //            }
        //            catch (Exception ex)
        //            {
        //                Dispatcher.Invoke(() =>
        //                {
        //                    txtNetworkLatency.Text = $"Ping情報を取得できませんでした: {ex.Message}";
        //                });
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        txtNetworkLatency.Text = $"Ping情報を取得できませんでした: {ex.Message}";
        //    }
        //}

        private string GetNetworkBandwidth()
        {
            string bandwidth = string.Empty;
            
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                
                foreach (NetworkInterface ni in interfaces)
                {
                    if (ni.OperationalStatus == OperationalStatus.Up)
                    {
                        IPv4InterfaceStatistics stats = ni.GetIPv4Statistics();
                        
                        long bytesSent = stats.BytesSent;
                        long bytesReceived = stats.BytesReceived;
                        
                        bandwidth += $"インターフェース: {ni.Name}\n";
                        bandwidth += $"送信: {FormatBytes(bytesSent)}\n";
                        bandwidth += $"受信: {FormatBytes(bytesReceived)}\n\n";
                    }
                }
            }
            catch
            {
                bandwidth = "帯域幅情報を取得できませんでした";
            }
            
            return bandwidth;
        }

        private string FormatBytes(long bytes)
        {
            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            
            for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }
            
            return String.Format("{0:0.##} {1}", dblSByte, suffix[i]);
        }
        #endregion

        #region 高度なセキュリティ情報
        //private void LoadAdvancedSecurityInfo()
        //{
        //    try
        //    {
        //        // パスワードの強度チェック
        //        txtPasswordStrength.Text = "パスワードの強度をチェックするには、[チェック]ボタンをクリックしてください。";
                
        //        // 不正なログイン試行の履歴
        //        txtLoginAttempts.Text = GetFailedLoginAttempts();
                
        //        // セキュリティソフトウェアの状態
        //        txtSecuritySoftware.Text = GetSecuritySoftwareStatus();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"高度なセキュリティ情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private string GetFailedLoginAttempts()
        {
            string loginAttempts = string.Empty;
            
            try
            {
                // セキュリティログから失敗したログイン試行を取得
                EventLog securityLog = new EventLog("Security");
                int count = 0;
                
                foreach (EventLogEntry entry in securityLog.Entries)
                {
                    if (entry.InstanceId == 4625 && count < 10) // イベントID 4625は失敗したログイン試行
                    {
                        loginAttempts += $"日時: {entry.TimeGenerated}\n";
                        loginAttempts += $"ユーザー: {GetUsernameFromSecurityEvent(entry.Message)}\n";
                        loginAttempts += $"ソース: {GetSourceFromSecurityEvent(entry.Message)}\n\n";
                        
                        count++;
                    }
                }
                
                if (count == 0)
                {
                    loginAttempts = "最近の不正なログイン試行は記録されていません。";
                }
            }
            catch
            {
                loginAttempts = "ログイン試行情報を取得できませんでした（管理者権限が必要な場合があります）";
            }
            
            return loginAttempts;
        }

        private string GetUsernameFromSecurityEvent(string message)
        {
            try
            {
                // セキュリティイベントのメッセージからユーザー名を抽出
                int accountNameIndex = message.IndexOf("アカウント名:");
                
                if (accountNameIndex != -1)
                {
                    int endOfLine = message.IndexOf('\n', accountNameIndex);
                    
                    if (endOfLine != -1)
                    {
                        return message.Substring(accountNameIndex + 6, endOfLine - accountNameIndex - 6).Trim();
                    }
                }
            }
            catch
            {
                // 解析エラー
            }
            
            return "不明";
        }

        private string GetSourceFromSecurityEvent(string message)
        {
            try
            {
                // セキュリティイベントのメッセージからソースIPを抽出
                int sourceAddressIndex = message.IndexOf("ソースネットワークアドレス:");
                
                if (sourceAddressIndex != -1)
                {
                    int endOfLine = message.IndexOf('\n', sourceAddressIndex);
                    
                    if (endOfLine != -1)
                    {
                        return message.Substring(sourceAddressIndex + 14, endOfLine - sourceAddressIndex - 14).Trim();
                    }
                }
            }
            catch
            {
                // 解析エラー
            }
            
            return "不明";
        }

        private string GetSecuritySoftwareStatus()
        {
            string securitySoftware = string.Empty;
            
            try
            {
                // インストールされているセキュリティソフトウェアの情報を取得
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\SecurityCenter2", "SELECT * FROM AntiVirusProduct"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string displayName = obj["displayName"].ToString();
                        string productState = obj["productState"].ToString();
                        
                        // 製品状態の解析（16進数の値でエンコードされています）
                        bool enabled = false;
                        bool upToDate = false;
                        
                        if (int.TryParse(productState, out int state))
                        {
                            enabled = ((state & 0x1000) == 0); // ビット12が0の場合、リアルタイム保護は有効
                            upToDate = ((state & 0x10) == 0);  // ビット4が0の場合、定義は最新
                        }
                        
                        securitySoftware += $"ソフトウェア: {displayName}\n";
                        securitySoftware += $"リアルタイム保護: {(enabled ? "有効" : "無効")}\n";
                        securitySoftware += $"定義の状態: {(upToDate ? "最新" : "更新が必要")}\n\n";
                    }
                }
                
                if (string.IsNullOrEmpty(securitySoftware))
                {
                    securitySoftware = "インストールされているセキュリティソフトウェアが見つかりませんでした。";
                }
            }
            catch
            {
                securitySoftware = "セキュリティソフトウェア情報を取得できませんでした";
            }
            
            return securitySoftware;
        }
        #endregion

        #region システムのリソース利用状況
        private void LoadResourceUsageInfo()
        {
            try
            {
                // スレッドとプロセスの詳細
                UpdateProcessInfo();

                // システムのヒープメモリ情報
                var dd = GetHeapMemoryInfo();

                Dispatcher.Invoke(() => {
                    txtHeapMemory.Text = dd;
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show($"リソース利用状況の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateProcessInfo()
        {
            try
            {
                // 上位5つのCPU使用プロセスを取得
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
                
                // 上位5つのメモリ使用プロセスを取得
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
                        double memoryMB = process.WorkingSet64 / (1024.0 * 1024);
                        processInfo += $"{process.ProcessName}: {memoryMB:F1} MB, スレッド数: {process.Threads.Count}\n";
                    }
                    catch
                    {
                        // プロセス情報を取得できない場合はスキップ
                    }
                }
                
                Dispatcher.Invoke(() => {
                txtThreadsProcesses.Text = processInfo;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => {
                txtThreadsProcesses.Text = $"プロセス情報を取得できませんでした: {ex.Message}";
                });
            }
        }

        private string GetHeapMemoryInfo()
        {
            string heapInfo = string.Empty;
            
            try
            {
                // メモリ情報を取得
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        ulong totalVirtualMemory = Convert.ToUInt64(obj["TotalVirtualMemorySize"]) * 1024;
                        ulong freeVirtualMemory = Convert.ToUInt64(obj["FreeVirtualMemory"]) * 1024;
                        ulong totalVisibleMemory = Convert.ToUInt64(obj["TotalVisibleMemorySize"]) * 1024;
                        ulong freePhysicalMemory = Convert.ToUInt64(obj["FreePhysicalMemory"]) * 1024;
                        
                        double totalVirtualGB = totalVirtualMemory / (1024.0 * 1024 * 1024);
                        double freeVirtualGB = freeVirtualMemory / (1024.0 * 1024 * 1024);
                        double totalVisibleGB = totalVisibleMemory / (1024.0 * 1024 * 1024);
                        double freePhysicalGB = freePhysicalMemory / (1024.0 * 1024 * 1024);
                        
                        heapInfo += $"総仮想メモリ: {totalVirtualGB:F2} GB\n";
                        heapInfo += $"空き仮想メモリ: {freeVirtualGB:F2} GB\n";
                        heapInfo += $"使用中仮想メモリ: {(totalVirtualGB - freeVirtualGB):F2} GB\n\n";
                        heapInfo += $"総物理メモリ: {totalVisibleGB:F2} GB\n";
                        heapInfo += $"空き物理メモリ: {freePhysicalGB:F2} GB\n";
                        heapInfo += $"使用中物理メモリ: {(totalVisibleGB - freePhysicalGB):F2} GB\n";
                    }
                }
            }
            catch
            {
                heapInfo = "ヒープメモリ情報を取得できませんでした";
            }
            
            return heapInfo;
        }
        #endregion

        #region バックアップと復元の状態
        //private void LoadBackupInfo()
        //{
        //    try
        //    {
        //        // バックアップの設定と履歴
        //        txtBackupStatus.Text = GetBackupStatus();
                
        //        // 復元ポイント
        //        txtRestorePoints.Text = GetRestorePoints();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"バックアップ情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private string GetBackupStatus()
        {
            string backupStatus = string.Empty;
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\Microsoft\Windows\Backup", "SELECT * FROM MsftWmiProxy_Service"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        bool isEnabled = Convert.ToBoolean(obj["IsEnabled"]);
                        
                        backupStatus += $"バックアップサービス: {(isEnabled ? "有効" : "無効")}\n";
                    }
                }
                
                if (string.IsNullOrEmpty(backupStatus))
                {
                    backupStatus = "Windowsバックアップ情報を取得できませんでした";
                }
            }
            catch
            {
                backupStatus = "Windowsバックアップ情報を取得できませんでした";
            }
            
            return backupStatus;
        }

        private string GetRestorePoints()
        {
            string restorePoints = string.Empty;
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root\DEFAULT", "SELECT * FROM SystemRestore"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string description = obj["Description"].ToString();
                        string type = obj["RestorePointType"].ToString();
                        string creationTime = ManagementDateTimeConverter.ToDateTime(obj["CreationTime"].ToString()).ToString();
                        string sequenceNumber = obj["SequenceNumber"].ToString();
                        
                        restorePoints += $"説明: {description}\n";
                        restorePoints += $"タイプ: {type}\n";
                        restorePoints += $"作成時間: {creationTime}\n";
                        restorePoints += $"シーケンス番号: {sequenceNumber}\n\n";
                    }
                }
                
                if (string.IsNullOrEmpty(restorePoints))
                {
                    restorePoints = "システムの復元ポイントが見つかりませんでした。";
                }
            }
            catch
            {
                restorePoints = "システム復元ポイント情報を取得できませんでした";
            }
            
            return restorePoints;
        }
        #endregion

        #region ドライバーとデバイスの状態
        private void LoadDriverInfo()
        {
            try
            {
                // インストールされているデバイスドライバのバージョンと状態
                var aa  = GetDriverVersions();
                Dispatcher.Invoke(() => {
                    txtDriverVersions.Text = aa;
                });

                
                // 不正なデバイス
                var bb = GetProblemDevices();
                Dispatcher.Invoke(() => {
                    txtProblemDevices.Text = bb;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ドライバー情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetDriverVersions()
        {
            string driverVersions = string.Empty;
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPSignedDriver"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string deviceName = obj["DeviceName"]?.ToString();
                        string driverVersion = obj["DriverVersion"]?.ToString();
                        string manufacturer = obj["Manufacturer"]?.ToString();
                        
                        if (!string.IsNullOrEmpty(deviceName) && !string.IsNullOrEmpty(driverVersion))
                        {
                            driverVersions += $"デバイス: {deviceName}\n";
                            driverVersions += $"メーカー: {manufacturer ?? "不明"}\n";
                            driverVersions += $"ドライババージョン: {driverVersion}\n\n";
                        }
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

        private string GetProblemDevices()
        {
            string problemDevices = string.Empty;
            
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE ConfigManagerErrorCode <> 0"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        string deviceName = obj["Name"]?.ToString();
                        string deviceID = obj["DeviceID"]?.ToString();
                        int errorCode = Convert.ToInt32(obj["ConfigManagerErrorCode"]);
                        
                        problemDevices += $"デバイス: {deviceName ?? "不明"}\n";
                        problemDevices += $"デバイスID: {deviceID ?? "不明"}\n";
                        problemDevices += $"エラーコード: {errorCode}\n\n";
                    }
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
        #endregion

        public ICommand CopyTextCommand => new RelayCommand(CopyText);

        private void CopyText(object parameter)
        {
            // テキストをコピーする処理
            if (parameter is TextBlock textBlock)
            {
                Clipboard.SetText(textBlock.Text);
            }
        }

        private async Task LoadIPInfoAsync()
        {
            try
            {
                // IPアドレス情報を取得
                var ipInfo = await GetIPInfoAsync();

                // 取得した情報を表示
                this.Dispatcher.Invoke(() =>
                {
                    IPAddressTextBlock.Text = ipInfo.Ip;
                    CountryTextBlock.Text = ipInfo.Country;
                    RegionTextBlock.Text = ipInfo.Region;
                    CityTextBlock.Text = ipInfo.City;
                    PostalTextBlock.Text = ipInfo.Postal;
                    TimezoneTextBlock.Text = ipInfo.Timezone;
                    ISPTextBlock.Text = ipInfo.Org;
                    LocationTextBlock.Text = $"{ipInfo.Loc} (緯度, 経度)";
                    IntializeWebView(ipInfo.Loc);
                });



            }
            catch (Exception ex)
            {
                MessageBox.Show($"情報の取得中にエラーが発生しました:\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
            }
        }

        private async Task<IPInfo> GetIPInfoAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                // ipinfo.ioのAPIを使用
                string response = await client.GetStringAsync("https://ipinfo.io/json");

                 // デシリアライズオプションを設定
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true, // プロパティ名の大文字小文字を無視
                            ReadCommentHandling = JsonCommentHandling.Skip, // コメントをスキップ
                            AllowTrailingCommas = true, // 末尾のカンマを許可
                        };
                return JsonSerializer.Deserialize<IPInfo>(response, options);
            }
        }
    }

    // IPアドレス情報のクラス
    public class IPInfo
    {
        public string Ip { get; set; }
        public string Hostname { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string Loc { get; set; }
        public string Org { get; set; }
        public string Postal { get; set; }
        public string Timezone { get; set; }

        public string Readme { get; set; }

        // その他の可能性のあるフィールド用に[JsonExtensionData]属性を使用
        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtensionData { get; set; }
    }


internal class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
}
}
