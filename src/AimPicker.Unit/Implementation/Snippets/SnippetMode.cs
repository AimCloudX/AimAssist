using AimPicker.Unit.Core.Mode;

namespace AimPicker.Unit.Implementation.Snippets
{
    public class SnippetMode : PikcerModeBase
    {
        private SnippetMode() : base(ModeName) { }

        public const string ModeName = "Snippet";

        public static SnippetMode Instance { get; } = new SnippetMode();

        public override string Prefix => "sn ";

        public override string Description => "スニペットモード";
    }
}
