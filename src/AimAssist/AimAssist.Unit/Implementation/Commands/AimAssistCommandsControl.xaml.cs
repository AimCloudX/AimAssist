using System.Windows;
using System.Windows.Controls;
using AimAssist.Core.Attributes;
using AimAssist.Core.Interfaces;

namespace AimAssist.Units.Implementation.Apps;

[AutoDataTemplate(typeof(AimAssistCommandsUnit))]
public partial class AimAssistCommandsControl : UserControl
{
    private readonly IAppCommands appCommands;

    public AimAssistCommandsControl(IAppCommands appCommands)
    {
        this.appCommands = appCommands;
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        appCommands.ShutdownAimAssist.Execute(this);
    }
}