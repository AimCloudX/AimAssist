using AimAssist.Units.Core.Mode;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace AimAssist.Units.Implementation.Knowledge
{
    public class KnowledgeMode : ModeBase
    {
        private KnowledgeMode() : base(ModeName) { }
        public override Control Icon => CreateIcon(PackIconKind.Wikipedia);

        public const string ModeName = "Knowledge";

        public static KnowledgeMode Instance { get; } = new KnowledgeMode();
        public override string Description => "AimAssist開発のナレッジを表示";
    }

}
