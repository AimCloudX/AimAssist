using AimAssist.Core.Attributes;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.WorkTools;

namespace AimAssist.Units.Implementation.Terminal
{
    [AutoRegisterUnit("Terminal", Priority = 95)]
    public class TerminalUnit : IUnit
    {
        public IMode Mode => WorkToolsMode.Instance;

        public string Name => "ターミナル";

        public string Description => "ConPTYを使用した高機能ターミナル";

        public string Category => "System";
    }
}