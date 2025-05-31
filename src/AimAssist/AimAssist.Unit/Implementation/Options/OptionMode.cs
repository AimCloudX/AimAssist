using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using AimAssist.Units.Core.Modes;

namespace AimAssist.Units.Implementation.Options
{
    public class OptionMode : ModeBase
    {
        private OptionMode() : base(ModeName)
        {
        }
        public override Control Icon => CreateIcon(PackIconKind.Settings);

        public const string ModeName = "Options";

        public static OptionMode Instance { get; } = new OptionMode();

        public override string Description => "AimAssistのオプション設定";
    }
}
