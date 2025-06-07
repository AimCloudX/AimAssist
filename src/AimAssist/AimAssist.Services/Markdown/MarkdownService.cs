using System.IO;

namespace AimAssist.Services.Markdown
{
    public class MarkdownService
    {
        public IEnumerable<MarkdownLink> GetLinks(string filePath)
        {
            var currentHeader = string.Empty;
            var inComment = false;
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.Trim().StartsWith("<!--"))
                {
                    inComment = true;
                    if (line.Trim().EndsWith("-->"))
                    {
                        inComment = false;
                    }
                    continue;
                }

                if (inComment)
                {
                    if (line.Trim().EndsWith("-->"))
                    {
                        inComment = false;
                    }
                    continue;
                }

                if (line.StartsWith("# "))
                {
                    currentHeader = line.Substring(2).Trim();
                }
                else if (line.Contains("[") && line.Contains("]("))
                {
                    var startText = line.IndexOf('[') + 1;
                    var endText = line.IndexOf(']', startText);
                    var startUrl = line.IndexOf('(', endText) + 1;
                    var endUrl = line.IndexOf(')', startUrl);

                    var text = line.Substring(startText, endText - startText);
                    var url = line.Substring(startUrl, endUrl - startUrl);

                    yield return new MarkdownLink(text, url, currentHeader);
                }
            }
        }

        public Dictionary<string, int> GetHeaderOrder(string filePath)
        {
            var headerOrder = new Dictionary<string, int>();
            bool inComment = false;
            int order = 0;
            
            foreach (var line in File.ReadAllLines(filePath))
            {
                if (line.Trim().StartsWith("<!--"))
                {
                    inComment = true;
                    if (line.Trim().EndsWith("-->"))
                    {
                        inComment = false;
                    }
                    continue;
                }

                if (inComment)
                {
                    if (line.Trim().EndsWith("-->"))
                    {
                        inComment = false;
                    }
                    continue;
                }

                if (line.StartsWith("# "))
                {
                    string header = line.Substring(2).Trim();
                    if (!headerOrder.ContainsKey(header))
                    {
                        headerOrder[header] = order++;
                    }
                }
            }
            
            return headerOrder;
        }
    }
}
