using System;
using System.Windows;
using System.Windows.Controls;

namespace AimAssist.Units.Implementation.Terminal
{
    public partial class ConPtyTerminalSession : UserControl, IDisposable
    {
        public string Title { get; set; } = "ConPTY Terminal";
        public string Shell { get; set; } = "pwsh.exe";

        public event EventHandler? ProcessExited;

        public ConPtyTerminalSession()
        {
            InitializeComponent();
            terminalControl.ProcessExited += (s, e) => ProcessExited?.Invoke(this, e);
            this.Loaded += UserControl_Loaded;
        }

        public ConPtyTerminalSession(string shell, string title) : this()
        {
            Shell = shell;
            Title = title;
            terminalControl.Shell = shell;
            terminalControl.Title = title;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            await terminalControl.StartTerminalAsync();
        }

        public void Dispose()
        {
            terminalControl?.Dispose();
        }
    }
}