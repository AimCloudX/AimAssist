using AimAssist.Unit.Core.Mode;

namespace AimAssist.Unit.Implementation.Web.Urls
{
    public class UrlMode : PikcerModeBase
    {
        private UrlMode() : base(ModeName) { }

        public const string ModeName = "URL";

        public static UrlMode Instance { get; } = new UrlMode();

        public override string Prefix => "https://";
        public override bool IsApplyFiter => false;
    }
}
