using AimAssist.Unit.Core.Mode;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace AimAssist.Unit.Implementation.Web.Urls
{
    public class UrlMode : ModeBase
    {
        private UrlMode() : base(ModeName) { }
        public override Control Icon => CreateIcon(PackIconKind.Link);

        public const string ModeName = "URL";

        public static UrlMode Instance { get; } = new UrlMode();

        public override bool IsApplyFiter => false;
    }
}
