using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AimAssist.Core.Attributes;

namespace AimAssist.Units.Implementation.Terminal;

[AutoDataTemplate(typeof(TerminalUnit))]
public partial class TerminalView : UserControl
{
    private int _tabCounter = 1;
    
    public TerminalView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        
        // Keyboard shortcuts
        KeyDown += OnKeyDown;
        Focusable = true;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Create initial tab
        CreateNewTab();
        Focus();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        // Handle keyboard shortcuts
        if (e.Key == Key.T && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && 
            (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
        {
            CreateNewTab();
            e.Handled = true;
        }
        else if (e.Key == Key.W && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && 
                 (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
        {
            CloseCurrentTab();
            e.Handled = true;
        }
        else if (e.Key >= Key.D1 && e.Key <= Key.D9 && 
                 (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            // Switch to tab by number (Ctrl+1-9)
            int tabIndex = (int)(e.Key - Key.D1);
            if (tabIndex < TerminalTabControl.Items.Count)
            {
                TerminalTabControl.SelectedIndex = tabIndex;
                e.Handled = true;
            }
        }
    }

    private void NewTab_Click(object sender, RoutedEventArgs e)
    {
        CreateNewTab();
    }

    private void CloseTab_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is TabItem tabItem)
        {
            CloseTab(tabItem);
        }
    }

    private void CloseCurrentTab_Click(object sender, RoutedEventArgs e)
    {
        CloseCurrentTab();
    }

    private void OpenSettings_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement settings dialog
        MessageBox.Show("設定機能は今後実装予定です。", "ターミナル設定", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void CreateNewTab()
    {
        var terminalControl = new ModernTerminalControl();
        
        var tabItem = new TabItem
        {
            Header = $"ターミナル {_tabCounter++}",
            Content = terminalControl
        };

        TerminalTabControl.Items.Add(tabItem);
        TerminalTabControl.SelectedItem = tabItem;

        // Focus the new terminal
        Dispatcher.BeginInvoke(new Action(() =>
        {
            terminalControl.Focus();
        }), System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void CloseTab(TabItem tabItem)
    {
        if (TerminalTabControl.Items.Count <= 1)
        {
            // Don't close the last tab, just clear it
            if (tabItem.Content is ModernTerminalControl terminal)
            {
                terminal.Reset();
            }
            return;
        }

        // Dispose terminal if needed
        if (tabItem.Content is ModernTerminalControl terminalToClose)
        {
            terminalToClose.Dispose();
        }

        TerminalTabControl.Items.Remove(tabItem);

        // If we closed the selected tab, select another one
        if (TerminalTabControl.SelectedItem == null && TerminalTabControl.Items.Count > 0)
        {
            TerminalTabControl.SelectedIndex = Math.Max(0, TerminalTabControl.Items.Count - 1);
        }
    }

    private void CloseCurrentTab()
    {
        if (TerminalTabControl.SelectedItem is TabItem selectedTab)
        {
            CloseTab(selectedTab);
        }
    }
}