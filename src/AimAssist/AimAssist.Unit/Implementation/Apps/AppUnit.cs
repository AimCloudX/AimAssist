using AimAssist.Core.Attributes;
using AimAssist.Core.Units;

namespace AimAssist.Units.Implementation.Apps;

[AutoRegisterUnit("AimAssist", Priority = 80)]
public class AppUnit : IUnit
{
    public IMode Mode { get; } = WorkTools.WorkToolsMode.Instance;
    public string Name { get; } = "AimAssist";
    public string Description { get; } = "AimAssist";
    public string Category { get; } = "AimAssist";
}