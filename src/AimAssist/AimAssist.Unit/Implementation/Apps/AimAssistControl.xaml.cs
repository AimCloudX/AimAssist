using System.Windows;
using System.Windows.Controls;
using AimAssist.Core.Attributes;
using AimAssist.Core.Interfaces;

namespace AimAssist.Units.Implementation.Apps;

[AutoDataTemplate(typeof(AppUnit))]
public partial class AimAssistControl : UserControl
{
    private readonly IAppCommands appCommands;

    public AimAssistControl(IAppCommands appCommands)
    {
        this.appCommands = appCommands;
        InitializeComponent();
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        appCommands.ShutdownAimAssist.Execute(this);
    }
}