using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AimAssist.Core.Attributes;
using AimAssist.Units.Implementation.Computer.Services;
using AimAssist.Units.Implementation.Computer.Commands;
using AimAssist.Units.Implementation.Computer.Helpers;

namespace AimAssist.Units.Implementation.Computer
{
    [AutoDataTemplate(typeof(ComputerUnit))]
    public partial class ComputerView : UserControl
    {
        private HardwareInfoService? hardwareInfoService;
        private NetworkInfoService? networkInfoService;
        private PerformanceInfoService? performanceInfoService;
        private SecurityInfoService? securityInfoService;
        private DetailedHardwareInfoService? detailedHardwareInfoService;
        private DriverInfoService? driverInfoService;
        private IpInfoService? ipInfoService;
        private ResourceUsageService? resourceUsageService;

        public ComputerView()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void TabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
            {
                var header = selectedTab.Header?.ToString();
                if (!string.IsNullOrEmpty(header))
                {
                    // Fire-and-forget is intentional for background data loading
                    _ = Task.Run(() => LoadTabDataAsync(header));
                }
            }
        }

        private async Task LoadTabDataAsync(string tabHeader)
        {
            try
            {
                switch (tabHeader)
                {
                    case "基本情報":
                        await LoadBasicInfoAsync();
                        break;
                    case "ハードウェア情報":
                        await LoadHardwareInfoAsync();
                        break;
                    case "ネットワーク情報":
                        await LoadNetworkInfoAsync();
                        break;
                    case "パフォーマンス情報":
                        await LoadPerformanceInfoAsync();
                        break;
                    case "セキュリティ":
                        await LoadSecurityInfoAsync();
                        break;
                    case "詳細なハードウェア情報":
                        await LoadDetailedHardwareInfoAsync();
                        break;
                    case "システムのリソース利用状況":
                        await LoadResourceUsageInfoAsync();
                        break;
                    case "ドライバとデバイスの状態":
                        await LoadDriverInfoAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"タブデータ読み込みエラー ({tabHeader}): {ex.Message}");
            }
        }

        private async Task LoadBasicInfoAsync()
        {
            try
            {
                hardwareInfoService ??= new HardwareInfoService();
                
                var basicData = await Task.Run(() => new
                {
                    ComputerName = Environment.MachineName,
                    Username = Environment.UserName,
                    Domain = Environment.UserDomainName,
                    Architecture = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit",
                    OsVersion = hardwareInfoService.GetOsVersionInfo(),
                    Hostname = System.Net.Dns.GetHostName(),
                    Uptime = TimeSpan.FromMilliseconds(Environment.TickCount)
                });

                await Dispatcher.InvokeAsync(() =>
                {
                    txtComputerName.Text = basicData.ComputerName;
                    txtUsername.Text = basicData.Username;
                    txtDomain.Text = basicData.Domain;
                    txtArchitecture.Text = basicData.Architecture;
                    txtOSVersion.Text = basicData.OsVersion;
                    txtHostname.Text = basicData.Hostname;
                    txtUptime.Text = $"{basicData.Uptime.Days}日 {basicData.Uptime.Hours}時間 {basicData.Uptime.Minutes}分";
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"基本情報取得エラー: {ex.Message}");
            }
        }

        private async Task LoadHardwareInfoAsync()
        {
            try
            {
                hardwareInfoService ??= new HardwareInfoService();
                
                var hardwareData = await Task.Run(() => new
                {
                    CpuInfo = hardwareInfoService.GetCpuInfo(),
                    MemoryInfo = hardwareInfoService.GetTotalPhysicalMemory(),
                    DiskInfo = DiskInfoHelper.GetDiskInfo(),
                    GpuInfo = hardwareInfoService.GetGpuInfo(),
                    MotherboardInfo = hardwareInfoService.GetMotherboardInfo()
                });

                await Dispatcher.InvokeAsync(() =>
                {
                    txtCPUInfo.Text = hardwareData.CpuInfo;
                    txtMemoryInfo.Text = hardwareData.MemoryInfo;
                    txtDiskInfo.Text = hardwareData.DiskInfo;
                    txtGPUInfo.Text = hardwareData.GpuInfo;
                    txtMotherboardInfo.Text = hardwareData.MotherboardInfo;
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ハードウェア情報取得エラー: {ex.Message}");
            }
        }

        private async Task LoadNetworkInfoAsync()
        {
            try
            {
                networkInfoService ??= new NetworkInfoService();
                
                var networkData = await Task.Run(() => new
                {
                    IpAddresses = networkInfoService.GetIpAddresses(),
                    MacAddress = networkInfoService.GetMacAddress(),
                    NetworkStatus = networkInfoService.GetNetworkStatus()
                });

                await Dispatcher.InvokeAsync(() =>
                {
                    txtIPv4Address.Text = networkData.IpAddresses.ContainsKey("IPv4") ? networkData.IpAddresses["IPv4"] : "取得できませんでした";
                    txtIPv6Address.Text = networkData.IpAddresses.ContainsKey("IPv6") ? networkData.IpAddresses["IPv6"] : "取得できませんでした";
                    txtMACAddress.Text = networkData.MacAddress;
                    txtNetworkStatus.Text = networkData.NetworkStatus;
                }, DispatcherPriority.Background);

                await LoadIpInfoAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ネットワーク情報取得エラー: {ex.Message}");
            }
        }

        private async Task LoadPerformanceInfoAsync()
        {
            try
            {
                performanceInfoService ??= new PerformanceInfoService();
                
                var performanceData = await Task.Run(() => new
                {
                    CpuUsage = performanceInfoService.GetCpuUsage(),
                    RamUsage = performanceInfoService.GetMemoryUsage(),
                    DiskUsage = performanceInfoService.GetDiskUsage(),
                });

                await Dispatcher.InvokeAsync(() =>
                {
                    txtCPUUsage.Text = $"{performanceData.CpuUsage:F1}%";
                    txtMemoryUsage.Text = $"{performanceData.RamUsage:F1}%";
                    txtDiskUsage.Text = performanceData.DiskUsage;
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"パフォーマンス情報取得エラー: {ex.Message}");
            }
        }

        private async Task LoadSecurityInfoAsync()
        {
            try
            {
                securityInfoService ??= new SecurityInfoService();
                
                var securityData = await Task.Run(() => new
                {
                    DefenderStatus = securityInfoService.GetWindowsDefenderStatus(),
                    FirewallStatus = securityInfoService.GetFirewallStatus(),
                    SecurityUpdates = securityInfoService.GetSecurityUpdateStatus()
                });

                await Dispatcher.InvokeAsync(() =>
                {
                    txtDefenderStatus.Text = securityData.DefenderStatus;
                    txtFirewallStatus.Text = securityData.FirewallStatus;
                    txtSecurityUpdates.Text = securityData.SecurityUpdates;
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"セキュリティ情報取得エラー: {ex.Message}");
            }
        }

        private async Task LoadDetailedHardwareInfoAsync()
        {
            try
            {
                detailedHardwareInfoService ??= new DetailedHardwareInfoService();
                
                var detailedHardwareData = await Task.Run(() => new
                {
                    CpuTemperature = detailedHardwareInfoService.GetCpuTemperature(),
                    GpuTemperature = detailedHardwareInfoService.GetGpuTemperature(),
                    StorageDetails = detailedHardwareInfoService.GetStorageDetails(),
                    BatteryStatus = detailedHardwareInfoService.GetBatteryStatus()
                });

                await Dispatcher.InvokeAsync(() =>
                {
                    txtCPUTemperature.Text = detailedHardwareData.CpuTemperature;
                    txtGPUTemperature.Text = detailedHardwareData.GpuTemperature;
                    txtStorageDetails.Text = detailedHardwareData.StorageDetails;
                    txtBatteryStatus.Text = detailedHardwareData.BatteryStatus;
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"詳細ハードウェア情報取得エラー: {ex.Message}");
            }
        }

        private async Task LoadResourceUsageInfoAsync()
        {
            try
            {
                resourceUsageService ??= new ResourceUsageService();
                
                var resourceData = await Task.Run(() => new
                {
                    ProcessInfo = resourceUsageService.GetProcessInfo(),
                    HeapMemory = resourceUsageService.GetHeapMemoryInfo()
                });

                await Dispatcher.InvokeAsync(() =>
                {
                    txtThreadsProcesses.Text = resourceData.ProcessInfo;
                    txtHeapMemory.Text = resourceData.HeapMemory;
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"リソース情報取得エラー: {ex.Message}");
            }
        }

        private async Task LoadDriverInfoAsync()
        {
            try
            {
                driverInfoService ??= new DriverInfoService();
                
                var driverData = await Task.Run(() => new
                {
                    DriverVersions = driverInfoService.GetDriverVersions(),
                    ProblemDevices = driverInfoService.GetProblemDevices()
                });

                await Dispatcher.InvokeAsync(() =>
                {
                    txtDriverVersions.Text = driverData.DriverVersions;
                    txtProblemDevices.Text = driverData.ProblemDevices;
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ドライバー情報取得エラー: {ex.Message}");
            }
        }

        public ICommand CopyTextCommand => new CopyTextCommand(CopyText);

        private void CopyText(object? parameter)
        {
            try
            {
                if (parameter is TextBlock textBlock && !string.IsNullOrEmpty(textBlock.Text))
                {
                    Clipboard.SetText(textBlock.Text);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"コピーエラー: {ex.Message}");
            }
        }

        private async Task LoadIpInfoAsync()
        {
            try
            {
                ipInfoService ??= new IpInfoService();
                
                var ipInfo = await ipInfoService.GetIpInfoAsync();

                await Dispatcher.InvokeAsync(() =>
                {
                    IPAddressTextBlock.Text = ipInfo.Ip ?? "取得できませんでした";
                    CountryTextBlock.Text = ipInfo.Country ?? "取得できませんでした";
                    RegionTextBlock.Text = ipInfo.Region ?? "取得できませんでした";
                    CityTextBlock.Text = ipInfo.City ?? "取得できませんでした";
                    PostalTextBlock.Text = ipInfo.Postal ?? "取得できませんでした";
                    TimezoneTextBlock.Text = ipInfo.Timezone ?? "取得できませんでした";
                    ISPTextBlock.Text = ipInfo.Org ?? "取得できませんでした";
                    LocationTextBlock.Text = !string.IsNullOrEmpty(ipInfo.Loc) ? $"{ipInfo.Loc} (緯度, 経度)" : "取得できませんでした";
                }, DispatcherPriority.Background);

                if (!string.IsNullOrEmpty(ipInfo.Loc))
                {
                    await Task.Delay(1000);
                    await InitializeWebViewAsync(ipInfo.Loc);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IP情報取得エラー: {ex.Message}");
            }
        }

        private async Task InitializeWebViewAsync(string loc)
        {
            try
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    await LocationWebView.EnsureCoreWebView2Async(null);
                    var url = $"https://www.google.com/maps?q={loc}";
                    LocationWebView.Source = new Uri(url);
                }, DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"WebView初期化エラー: {ex.Message}");
            }
        }
    }
}
