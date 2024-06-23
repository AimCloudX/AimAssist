using AimAssist.Unit.Core.Mode;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace AimAssist.Unit.Implementation.Web.Rss
{
    public class RssMode : PikcerModeBase
    {
        private RssMode() : base(ModeName)
        {
        }

        public override Control Icon => CreateIcon(PackIconKind.Journal);

        public const string ModeName = "Rss";

        public static RssMode Instance { get; } = new RssMode();

        public override string Description => "主要ニュースサイトRSSから最新記事の取得";

        public override bool IsApplyFiter => false; 
    }
}
