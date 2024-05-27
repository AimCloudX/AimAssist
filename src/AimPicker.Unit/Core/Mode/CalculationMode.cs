namespace AimPicker.Unit.Core.Mode
{
    public class CalculationMode : PikcerModeBase
    {
        private CalculationMode() : base(ModeName) { }

        public const string ModeName = "Calculation";

        public static CalculationMode Instance { get; } = new CalculationMode();

        public override string Prefix => "=";
    }
}
