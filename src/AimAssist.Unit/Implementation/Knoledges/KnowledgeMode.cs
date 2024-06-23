using AimAssist.Unit.Core.Mode;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace AimAssist.Unit.Implementation.Knowledge
{
    public class KnowledgeMode : PikcerModeBase
    {
        private KnowledgeMode() : base(ModeName) { }
        public override Control Icon => CreateIcon(PackIconKind.Wikipedia);

        public const string ModeName = "Knowledge";

        public static KnowledgeMode Instance { get; } = new KnowledgeMode();
        public override string Description => "AimAssist開発のナレッジを表示";
    }

}
