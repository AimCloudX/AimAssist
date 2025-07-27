using AimAssist.Core.Attributes;
using AimAssist.Core.Units;

namespace AimAssist.Units.Implementation.Terminal
{
    [AutoRegisterUnit("Terminal", Priority = 95)]
    public class TerminalUnit : IUnit
    {
        public IMode Mode => TerminalMode.Instance;

        public string Name => "ターミナル";

        public string Description => "PowerShell (pwsh) ベースの高機能ターミナル";

        public string Category => "System";
    }
}