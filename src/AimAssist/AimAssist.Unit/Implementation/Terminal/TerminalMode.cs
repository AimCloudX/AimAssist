using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using AimAssist.Core.Attributes;
using AimAssist.Units.Core.Modes;

namespace AimAssist.Units.Implementation.Terminal
{
    [ModeDisplayOrder(20)]
    public class TerminalMode : ModeBase
    {
        private TerminalMode() : base(ModeName) { }

        public const string ModeName = "Terminal";

        public static TerminalMode Instance { get; } = new TerminalMode();

        public override Control Icon => CreateIcon(PackIconKind.Terminal);

        public override string Description => "ターミナル・コンソール機能";
    }
}