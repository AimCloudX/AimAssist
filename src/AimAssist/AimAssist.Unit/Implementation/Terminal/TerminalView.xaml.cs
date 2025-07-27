using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AimAssist.Core.Attributes;

namespace AimAssist.Units.Implementation.Terminal;

[AutoDataTemplate(typeof(TerminalUnit))]
public partial class TerminalView : UserControl
{
    public TerminalView()
    {
        InitializeComponent();
    }
}