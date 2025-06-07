using AimAssist.Core.Attributes;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Units.Implementation.ClipboardAnalyzer;

[AutoRegisterUnit("System", Priority = 85)]
public class ClipboardUnit : IUnit
{
    public IMode Mode => WorkToolsMode.Instance;

    public string Name => "ClipboardAnalyzer";

    public string Description =>
        System.Windows.Clipboard.ContainsText() ? System.Windows.Clipboard.GetText() : string.Empty;

    public string Category => string.Empty;
}
