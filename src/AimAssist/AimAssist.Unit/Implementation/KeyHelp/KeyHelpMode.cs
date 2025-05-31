using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using AimAssist.Units.Core.Modes;

namespace AimAssist.Units.Implementation.KeyHelp
{
    public class KeyHelpMode : ModeBase
    {
        private KeyHelpMode() : base(ModeName) { }

        public override Control Icon => CreateIcon(PackIconKind.Keyboard);
        public const string ModeName = "KeyHelp";

        public static KeyHelpMode Instance { get; } = new KeyHelpMode();

        public override string Description => "ショートカットキーの提案";

        public override bool IsIncludeAllInclusive => false;
    }
}
