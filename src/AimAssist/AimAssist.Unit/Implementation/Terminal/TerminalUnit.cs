using AimAssist.Core.Attributes;
using AimAssist.Core.Units;

namespace AimAssist.Units.Implementation.Terminal
{
    [AutoRegisterUnit("Terminal", Priority = 95)]
    public class TerminalUnit : IUnit
    {
        public IMode Mode => TerminalMode.Instance;

        public string Name => "ターミナル";

        public string Description => "vs-pty.netを使用した高機能ターミナル（dirコマンド対応）";

        public string Category => "System";
    }
}