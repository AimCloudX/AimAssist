using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace AimAssist.Unit.Core.Mode
{
    public class CalculationMode : PikcerModeBase
    {
        private CalculationMode() : base(ModeName) { }

        public const string ModeName = "Calculation";
        public override Control Icon => CreateIcon(PackIconKind.Calculator);

        public static CalculationMode Instance { get; } = new CalculationMode();
    }
}
