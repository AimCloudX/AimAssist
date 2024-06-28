using AimAssist.Units.Core.Mode;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace AimAssist.Units.Implementation.Snippets
{
    public class SnippetMode : ModeBase
    {
        private SnippetMode() : base(ModeName) { }

        public override Control Icon => CreateIcon(PackIconKind.Text);
        public const string ModeName = "Snippet";

        public static SnippetMode Instance { get; } = new SnippetMode();


        public override string Description => "スニペットモード";
    }
}
