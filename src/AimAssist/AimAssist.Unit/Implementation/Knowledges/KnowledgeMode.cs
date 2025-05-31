using System.Windows.Controls;
using AimAssist.Units.Core.Modes;
using MaterialDesignThemes.Wpf;

namespace AimAssist.Units.Implementation.Knowledges
{
    public class KnowledgeMode : ModeBase
    {
        private KnowledgeMode() : base(ModeName) { }
        public override Control Icon => CreateIcon(PackIconKind.Encyclopedia);

        public const string ModeName = "Knowledge";

        public static KnowledgeMode Instance { get; } = new KnowledgeMode();
        public override string Description => "AimAssist開発のナレッジを表示";
    }

}
