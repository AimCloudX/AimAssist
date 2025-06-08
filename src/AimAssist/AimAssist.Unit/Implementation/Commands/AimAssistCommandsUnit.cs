using AimAssist.Core;
using AimAssist.Core.Attributes;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Units.Implementation.Apps;

[AutoRegisterUnit(Constants.AppName, Priority = 80)]
public class AimAssistCommandsUnit : IUnit
{
    public IMode Mode { get; } = WorkToolsMode.Instance;
    public string Name { get; } = "Commands";
    public string Description { get; } = "";
    public string Category { get; } = Constants.AppName;
}