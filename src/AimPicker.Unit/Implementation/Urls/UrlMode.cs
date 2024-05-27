using AimPicker.Unit.Core.Mode;

namespace AimPicker.Unit.Implementation.Urls
{
    public class UrlMode : PikcerModeBase
    {
        private UrlMode() : base(ModeName) { }

        public const string ModeName = "URL";

        public static UrlMode Instance { get; } = new UrlMode();

        public override string Prefix => "https://";
    }
}
