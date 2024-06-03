using AimAssist.Unit.Core.Mode;

namespace AimAssist.Unit.Implementation.Standard
{
    public class StandardMode : PikcerModeBase
    {
        private StandardMode() : base(ModeName) { }

        public const string ModeName = "AimAssist";

        public static StandardMode Instance { get; } = new StandardMode();

        public override string Prefix => "";

        public override string Description => "モード選択";
    }
}
