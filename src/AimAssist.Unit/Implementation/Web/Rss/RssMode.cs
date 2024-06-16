using AimAssist.Unit.Core.Mode;

namespace AimAssist.Unit.Implementation.Web.Rss
{
    internal class RssMode : PikcerModeBase
    {
        private RssMode() : base(ModeName)
        {
        }

        public const string ModeName = "Rss";

        public static RssMode Instance { get; } = new RssMode();

        public override string Prefix => "rss ";
        public override string Description => "主要ニュースサイトRSSから最新記事の取得";

        public override bool IsApplyFiter => false; 
    }
}
