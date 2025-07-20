using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AimAssist.Core.Attributes;

namespace AimAssist.Units.Implementation.Terminal
{
    [AutoDataTemplate(typeof(TerminalUnit))]
    public partial class TerminalView : UserControl
    {
        private int _tabCounter = 1;
        private Dictionary<TabItem, TerminalSession> _terminals = new Dictionary<TabItem, TerminalSession>();

        public TerminalView()
        {
            try
            {
                InitializeComponent();
                _terminals = new Dictionary<TabItem, TerminalSession>();
                this.Loaded += TerminalView_Loaded;
                this.Unloaded += TerminalView_Unloaded;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TerminalView constructor error: {ex}");
                MessageBox.Show($"ターミナルビューの初期化エラー: {ex.Message}\n\n{ex.StackTrace}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void TerminalView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // WSLディストリビューションを動的に追加
                await PopulateShellDropdownAsync();
                
                // 初回起動時に自動的に1つのタブを開く
                if (tabControl.Items.Count == 0)
                {
                    AddNewTab();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TerminalView_Loaded error: {ex}");
                MessageBox.Show($"ターミナルビューの初期化エラー: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task PopulateShellDropdownAsync()
        {
            try
            {
                // 既存のアイテムをクリア
                cmbShellType.Items.Clear();

                // 基本的なシェルを追加
                var powershellItem = new ComboBoxItem { Content = "PowerShell", IsSelected = true };
                cmbShellType.Items.Add(powershellItem);
                cmbShellType.Items.Add(new ComboBoxItem { Content = "コマンドプロンプト" });

                // WSLディストリビューションを非同期で取得して追加
                try
                {
                    await Task.Run(async () =>
                    {
                        if (WslHelper.IsWslInstalled())
                        {
                            var distributions = await WslHelper.GetInstalledDistributionsAsync();
                            
                            // UI操作はメインスレッドで実行
                            await Dispatcher.InvokeAsync(() =>
                            {
                                foreach (var distro in distributions.Where(d => d.State.Equals("Running", StringComparison.OrdinalIgnoreCase) || 
                                                                              d.State.Equals("Stopped", StringComparison.OrdinalIgnoreCase)))
                                {
                                    var displayName = distro.IsDefault ? $"WSL ({distro.Name}) *" : $"WSL ({distro.Name})";
                                    var item = new ComboBoxItem 
                                    { 
                                        Content = displayName,
                                        Tag = distro.Name
                                    };
                                    cmbShellType.Items.Add(item);
                                }

                                // ディストリビューションが見つからない場合は汎用WSLを追加
                                if (!distributions.Any())
                                {
                                    cmbShellType.Items.Add(new ComboBoxItem { Content = "WSL" });
                                }
                            });
                        }
                    });
                }
                catch (Exception ex)
                {
                    // WSL検出でエラーが発生しても続行
                    System.Diagnostics.Debug.WriteLine($"WSL detection error: {ex.Message}");
                }

                // デフォルト選択を設定
                if (cmbShellType.SelectedItem == null && cmbShellType.Items.Count > 0)
                {
                    cmbShellType.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                // エラーが発生した場合は基本的なシェルのみを追加
                System.Diagnostics.Debug.WriteLine($"PopulateShellDropdown error: {ex.Message}");
                
                // 最低限PowerShellとコマンドプロンプトは使えるようにする
                cmbShellType.Items.Clear();
                cmbShellType.Items.Add(new ComboBoxItem { Content = "PowerShell", IsSelected = true });
                cmbShellType.Items.Add(new ComboBoxItem { Content = "コマンドプロンプト" });
                cmbShellType.SelectedIndex = 0;
            }
        }

        private void TerminalView_Unloaded(object sender, RoutedEventArgs e)
        {
            // すべてのターミナルセッションをクリーンアップ
            foreach (var terminal in _terminals.Values)
            {
                terminal.Dispose();
            }
            _terminals.Clear();
        }

        private void BtnAddTab_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab();
        }

        private void AddNewTab()
        {
            // Get selected shell type and executable
            string shellExecutable = "pwsh.exe";
            string shellDisplayName = "PowerShell";
            
            if (cmbShellType.SelectedItem is ComboBoxItem selectedItem)
            {
                var selectedText = selectedItem.Content.ToString() ?? "";
                
                if (selectedText == "コマンドプロンプト")
                {
                    shellExecutable = "cmd.exe";
                    shellDisplayName = "コマンドプロンプト";
                }
                else if (selectedText.StartsWith("WSL"))
                {
                    shellDisplayName = "WSL";
                    
                    // WSLディストリビューション名を取得
                    if (selectedItem.Tag is string distroName && !string.IsNullOrEmpty(distroName))
                    {
                        shellExecutable = $"wsl.exe -d {distroName}";
                        shellDisplayName = $"WSL ({distroName})";
                    }
                    else
                    {
                        shellExecutable = "wsl.exe";
                    }
                }
                else
                {
                    shellExecutable = "pwsh.exe";
                    shellDisplayName = "PowerShell";
                }
            }

            var sessionName = $"{shellDisplayName} {_tabCounter++}";
            var terminalSession = new TerminalSession(sessionName, shellExecutable);
            
            // Create tab header with close button
            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            headerPanel.Children.Add(new TextBlock { Text = sessionName, Margin = new Thickness(0, 0, 5, 0) });
            
            var closeButton = new Button
            {
                Content = "×",
                Background = System.Windows.Media.Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = System.Windows.Media.Brushes.Red,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(2, 0, 2, 0),
                MinWidth = 16
            };
            
            var tabItem = new TabItem
            {
                Header = headerPanel,
                Content = terminalSession
            };
            
            closeButton.Tag = tabItem;
            closeButton.Click += CloseTab_Click;
            headerPanel.Children.Add(closeButton);

            _terminals[tabItem] = terminalSession;
            tabControl.Items.Add(tabItem);
            tabControl.SelectedItem = tabItem;

            UpdateStatus($"{sessionName} を開きました");
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is TabItem tabItem)
            {
                if (_terminals.TryGetValue(tabItem, out var terminal))
                {
                    terminal.Dispose();
                    _terminals.Remove(tabItem);
                }

                tabControl.Items.Remove(tabItem);
                
                if (tabControl.Items.Count == 0)
                {
                    UpdateStatus("すべてのターミナルが閉じられました");
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl.SelectedItem is TabItem selectedTab && _terminals.ContainsKey(selectedTab))
            {
                var session = _terminals[selectedTab];
                UpdateStatus($"{session.SessionName} がアクティブです");
            }
        }

        private void UpdateStatus(string message)
        {
            txtStatus.Text = message;
        }
    }
}