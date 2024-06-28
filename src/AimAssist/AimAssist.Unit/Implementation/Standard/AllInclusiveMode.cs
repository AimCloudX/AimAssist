using AimAssist.Units.Core.Mode;
using Common.Commands.Shortcus;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Input;

namespace AimAssist.Units.Implementation.Standard
{
    public class AllInclusiveMode : ModeBase
    {
        private AllInclusiveMode() : base(ModeName) { }

        public override Control Icon => CreateIcon(PackIconKind.AllInclusive);
        public const string ModeName = "AllInclusive";

        public static AllInclusiveMode Instance { get; } = new AllInclusiveMode();

        public override string Description => "";
        public override KeySequence DefaultKeySequence =>  new KeySequence(Key.K, ModifierKeys.Control, Key.I, ModifierKeys.Control);
    }
}
