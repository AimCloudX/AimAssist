namespace AimPicker.Unit.Core.Mode
{
    public class NormalMode : PikcerModeBase
    {
        private NormalMode() : base(ModeName) { }

        public const string ModeName = "AimPicker";

        public static NormalMode Instance { get; } = new NormalMode();

        public override string Prefix => "";

        public override string Description => "モード選択";
    }
}
