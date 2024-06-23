using AimAssist.Unit.Core.Mode;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace AimAssist.Unit.Implementation.Standard
{
    public class StandardMode : PikcerModeBase
    {
        private StandardMode() : base(ModeName) { }

        public override Control Icon => CreateIcon(PackIconKind.AllInclusive);
        public const string ModeName = "AllInclusive";

        public static StandardMode Instance { get; } = new StandardMode();

        public override string Description => "";
    }
}
