namespace AimAssist.Services.Markdown
{
    public class MarkdownLink(string text, string url, string containingHeader = "")
    {
        public string Text { get; } = text;
        public string Url { get; } = url;
        public string ContainingHeader { get; } = containingHeader;

        public override string ToString()
    {
        return $"[{Text}]({Url}) - In header: {ContainingHeader}";
    }
    }
}
