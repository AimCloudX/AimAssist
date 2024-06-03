using AimAssist.Unit.Core.Mode;

namespace AimAssist.Unit.Implementation.Wiki
{
    public class KnowledgeMode : PikcerModeBase
    {
        private KnowledgeMode() : base(ModeName) { }

        public const string ModeName = "Knowledge";

        public static KnowledgeMode Instance { get; } = new KnowledgeMode();

        public override string Prefix => "kn ";
        public override string Description => "AimAssist開発のナレッジ";
    }
}
