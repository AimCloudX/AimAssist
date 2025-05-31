using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using AimAssist.Units.Core.Modes;

namespace AimAssist.Units.Implementation.Web.Rss
{
    public class RssMode : ModeBase
    {
        private RssMode() : base(ModeName)
        {
        }

        public override Control Icon => CreateIcon(PackIconKind.Rss);

        public const string ModeName = "Rss";

        public static RssMode Instance { get; } = new RssMode();

        public override string Description => "主要ニュースサイトRSSから最新記事の取得";
    }
}
