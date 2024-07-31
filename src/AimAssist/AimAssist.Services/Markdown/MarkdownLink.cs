using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Services.Markdown
{
    public class MarkdownLink
    {
        public string Text { get; set; }
        public string Url { get; set; }
    public string ContainingHeader { get; set; }

    public MarkdownLink(string text, string url, string containingHeader = null)
    {
        Text = text;
        Url = url;
        ContainingHeader = containingHeader;
    }

    public override string ToString()
    {
        return $"[{Text}]({Url}) - In header: {ContainingHeader ?? "Not in a header"}";
    }
    }
}
