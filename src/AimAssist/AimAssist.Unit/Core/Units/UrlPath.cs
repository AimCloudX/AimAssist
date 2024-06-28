namespace AimAssist.Units.Core.Units
{
    public class UrlPath : IUnitContent
    {
        public UrlPath(string url)
        {
            Url = url;
        }

        public string Url { get; }
    }
}
