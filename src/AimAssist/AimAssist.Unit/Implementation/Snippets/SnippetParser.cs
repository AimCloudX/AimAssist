using System.IO;
using System.Text.RegularExpressions;
using System.Management.Automation;
using System.Collections.Concurrent;

namespace AimAssist.Units.Implementation.Snippets
{
    public class SnippetParser
    {
        public List<Snippet> ParseMarkdownFile(string filePath)
        {
            var snippets = new ConcurrentBag<Snippet>();
            var fileContent = File.ReadAllText(filePath);

            // Split the content by snippet headers (##)
            var snippetSections = Regex.Split(fileContent, @"(?=^## )", RegexOptions.Multiline)
                                       .Where(s => !string.IsNullOrWhiteSpace(s))
                                       .Skip(1); // Skip the file header

            // TODO 非同期
            snippetSections.AsParallel().ForAll(s => { 
                var lines = s.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length < 3) return; // Skip invalid sections

                var name = lines[0].Trim('#', ' ').TrimEnd();
                var categoryMatch = Regex.Match(lines[1], @"\*\*Category:\*\* (.+)");
                var category = categoryMatch.Success ? categoryMatch.Groups[1].Value : string.Empty;
                var content = string.Join("\n", lines.SkipWhile(l => !l.StartsWith("```")).Skip(1).TakeWhile(l => !l.StartsWith("```"))).Trim();

                var executeContent = ExecuteSnippet(content);
                snippets.Add(new Snippet(name, executeContent, category));

            });

            return snippets.ToList();
        }

        private string ExecuteSnippet(string code)
        {
            var powerShellRegex = new Regex(@"{{(.+?)}}");
            return powerShellRegex.Replace(code, match => ExecutePowerShellCode(match.Groups[1].Value.Trim()));
        }

        private string ExecutePowerShellCode(string code)
        {
            using (var powershell = PowerShell.Create())
            {
                powershell.AddScript(code);

                try
                {
                    var results = powershell.Invoke();
                    return string.Join(Environment.NewLine, results);
                }
                catch (Exception ex)
                {
                    return $"Error executing PowerShell code: {ex.Message}";
                }
            }
        }
    }
}
