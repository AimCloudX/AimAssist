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
        private readonly DispatcherTimer timer;
        private readonly HardwareInfoService hardwareInfoService;
        private readonly NetworkInfoService networkInfoService;
        private readonly PerformanceInfoService performanceInfoService;
        private readonly SecurityInfoService securityInfoService;
        private readonly DetailedHardwareInfoService detailedHardwareInfoService;
        private readonly DriverInfoService driverInfoService;
        private readonly IpInfoService ipInfoService;
        private readonly ResourceUsageService resourceUsageService;

        public ComputerView()
        {
            InitializeComponent();
            this.DataContext = this;

            hardwareInfoService = new HardwareInfoService();
            networkInfoService = new NetworkInfoService();
            performanceInfoService = new PerformanceInfoService();
            securityInfoService = new SecurityInfoService();
            detailedHardwareInfoService = new DetailedHardwareInfoService();
            driverInfoService = new DriverInfoService();
            ipInfoService = new IpInfoService();
            resourceUsageService = new ResourceUsageService();

            LoadData();
            
            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            timer.Tick += TimerTick;
            timer.Start();
        }

        private async void InitializeWebView(string loc)
        {
            await LocationWebView.EnsureCoreWebView2Async(null);
            var url = $"https://www.google.com/maps?q={loc}";
            LocationWebView.Source = new Uri(url);
        }

        private void TimerTick(object? sender, EventArgs e)
        {
            Task.Run(() =>
            {
                UpdatePerformanceInfo();
                UpdateNetworkStatus();
            });
        }

        private void LoadData()
        {
            Task.Run(() =>
            {
                LoadBasicInfo();
                LoadHardwareInfo();
                LoadNetworkInfo();
                LoadPerformanceInfo();
                LoadSecurityInfo();
                LoadDetailedHardwareInfo();
                LoadResourceUsageInfo();
                LoadDriverInfo();
            });
        }

        private void LoadBasicInfo()
        {
            try
            {
                string osVersion = hardwareInfoService.GetOsVersionInfo();
                string architecture = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
                TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);

                Dispatcher.Invoke(() => {
                    txtComputerName.Text = Environment.MachineName;
                    txtOSVersion.Text = osVersion;
                    txtArchitecture.Text = architecture;
                    txtUsername.Text = Environment.UserName;
                    txtHostname.Text = System.Net.Dns.GetHostName();
                    txtDomain.Text = Environment.UserDomainName;
                    txtUptime.Text = $"{uptime.Days}日 {uptime.Hours}時間 {uptime.Minutes}分 {uptime.Seconds}秒";
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"基本情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadHardwareInfo()
        {
            try
            {
                string cpuInfo = hardwareInfoService.GetCpuInfo();
                string memoryInfo = hardwareInfoService.GetTotalPhysicalMemory();
                string diskInfo = DiskInfoHelper.GetDiskInfo();
                string gpuInfo = hardwareInfoService.GetGpuInfo();
                string motherboardInfo = hardwareInfoService.GetMotherboardInfo();

                Dispatcher.Invoke(() => {
                    txtCPUInfo.Text = cpuInfo;
                    txtMemoryInfo.Text = memoryInfo;
                    txtDiskInfo.Text = diskInfo;
                    txtGPUInfo.Text = gpuInfo;
                    txtMotherboardInfo.Text = motherboardInfo;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ハードウェア情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadNetworkInfo()
        {
            try
            {
                Dictionary<string, string> ipAddresses = networkInfoService.GetIpAddresses();
                string macAddress = networkInfoService.GetMacAddress();

                Dispatcher.Invoke(() => {
                    txtIPv4Address.Text = ipAddresses.ContainsKey("IPv4") ? ipAddresses["IPv4"] : "取得できませんでした";
                    txtIPv6Address.Text = ipAddresses.ContainsKey("IPv6") ? ipAddresses["IPv6"] : "取得できませんでした";
                    txtMACAddress.Text = macAddress;
                });
                
                UpdateNetworkStatus();
                _ = LoadIpInfoAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ネットワーク情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadPerformanceInfo()
        {
            try
            {
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
                float cpuUsage = performanceInfoService.GetCpuUsage();
                float ramUsage = performanceInfoService.GetMemoryUsage();
                string diskUsage = performanceInfoService.GetDiskUsage();
                string serviceStatus = performanceInfoService.GetServiceStatus();

                Dispatcher.Invoke(() =>
                {
                    txtCPUUsage.Text = $"{cpuUsage:F1}%";
                    txtMemoryUsage.Text = $"{ramUsage:F1}%";
                    txtDiskUsage.Text = diskUsage;
                    txtServiceStatus.Text = serviceStatus;

                    TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount);
                    txtUptime.Text = $"{uptime.Days}日 {uptime.Hours}時間 {uptime.Minutes}分 {uptime.Seconds}秒";
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"パフォーマンス情報の更新中にエラーが発生しました: {ex.Message}");
            }
        }

        private void UpdateNetworkStatus()
        {
            try
            {
                string networkStatus = networkInfoService.GetNetworkStatus();
                Dispatcher.Invoke(() => {
                    txtNetworkStatus.Text = networkStatus;
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => {
                    txtNetworkStatus.Text = $"接続情報を取得できませんでした: {ex.Message}";
                });
            }
        }

        private void LoadSecurityInfo()
        {
            try
            {
                string defenderStatus = securityInfoService.GetWindowsDefenderStatus();
                string firewallStatus = securityInfoService.GetFirewallStatus();
                string securityUpdates = securityInfoService.GetSecurityUpdateStatus();

                Dispatcher.Invoke(() => {
                    txtDefenderStatus.Text = defenderStatus;
                    txtFirewallStatus.Text = firewallStatus;
                    txtSecurityUpdates.Text = securityUpdates;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"セキュリティ情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDetailedHardwareInfo()
        {
            try
            {
                string cpuTemperature = detailedHardwareInfoService.GetCpuTemperature();
                string gpuTemperature = detailedHardwareInfoService.GetGpuTemperature();
                string storageDetails = detailedHardwareInfoService.GetStorageDetails();
                string batteryStatus = detailedHardwareInfoService.GetBatteryStatus();

                Dispatcher.Invoke(() => {
                    txtCPUTemperature.Text = cpuTemperature;
                    txtGPUTemperature.Text = gpuTemperature;
                    txtStorageDetails.Text = storageDetails;
                    txtBatteryStatus.Text = batteryStatus;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"詳細なハードウェア情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadResourceUsageInfo()
        {
            try
            {
                string processInfo = resourceUsageService.GetProcessInfo();
                string heapMemory = resourceUsageService.GetHeapMemoryInfo();

                Dispatcher.Invoke(() => {
                    txtThreadsProcesses.Text = processInfo;
                    txtHeapMemory.Text = heapMemory;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"リソース利用状況の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDriverInfo()
        {
            try
            {
                string driverVersions = driverInfoService.GetDriverVersions();
                string problemDevices = driverInfoService.GetProblemDevices();

                Dispatcher.Invoke(() => {
                    txtDriverVersions.Text = driverVersions;
                    txtProblemDevices.Text = problemDevices;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ドライバー情報の取得中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ICommand CopyTextCommand => new CopyTextCommand(CopyText);

        private void CopyText(object? parameter)
        {
            if (parameter is TextBlock textBlock)
            {
                Clipboard.SetText(textBlock.Text);
            }
        }

        private async Task LoadIpInfoAsync()
        {
            try
            {
                var ipInfo = await ipInfoService.GetIpInfoAsync();

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
                    InitializeWebView(ipInfo.Loc);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"情報の取得中にエラーが発生しました:\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
