using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace AimAssist.Services.Markdown
{
    public class MarkdownService
    {
        public IEnumerable<MarkdownLink> GetLinks(string filePath)
        {
            string currentHeader = null;
            bool inComment = false;
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
                    int startText = line.IndexOf("[") + 1;
                    int endText = line.IndexOf("]", startText);
                    int startUrl = line.IndexOf("(", endText) + 1;
                    int endUrl = line.IndexOf(")", startUrl);

                    string text = line.Substring(startText, endText - startText);
                    string url = line.Substring(startUrl, endUrl - startUrl);

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
