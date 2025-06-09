using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using AimAssist.Core.Attributes;
using AimAssist.Units.Core.Modes;

namespace AimAssist.Units.Implementation.Snippets
{
    [ModeDisplayOrder(60)]
    public class SnippetMode : ModeBase
    {
        private SnippetMode() : base(ModeName) { }

        public override Control Icon => CreateIcon(PackIconKind.Text);
        public const string ModeName = "Snippet";

        public static SnippetMode Instance { get; } = new SnippetMode();


        public override string Description => "スニペットモード";
    }
}
