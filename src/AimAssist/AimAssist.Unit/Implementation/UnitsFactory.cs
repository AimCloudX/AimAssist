using AimAssist.Services.Markdown;
using AimAssist.Units.Core;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.ApplicationLog;
using AimAssist.Units.Implementation.Knowledge;
using AimAssist.Units.Implementation.Options;
using AimAssist.Units.Implementation.Snippets;
using AimAssist.Units.Implementation.Web.BookSearch;
using AimAssist.Units.Implementation.Web.Rss;
using AimAssist.Units.Implementation.WorkTools;
using System.IO;

namespace AimAssist.Units.Implementation
{
    public class UnitsFactory : IUnitsFacotry
    {
        public IEnumerable<IUnit> GetUnits()
        {
            string roamingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string targetPath = Path.Combine(roamingPath, "AimAssist", "WorkItem.md");

            var links  = new MarkdownService().GetLinks(targetPath);

            foreach (var link in links)
            {
                yield return new UrlUnit(WorkToolsMode.Instance, link.Text, link.Url, link.ContainingHeader);
            }

            yield return new TranscriptionUnit();
            yield return new BookSearchSettingUnit();
            yield return new RssSettingUnit();
            yield return new AppLogUnit();

            var dictInfo = new DirectoryInfo("Resources/Knowledge/");
            foreach (var file in dictInfo.GetFiles())
            {
                yield return new MarkdownUnit(file, string.Empty, KnowledgeMode.Instance);
            }

            foreach (var directory in dictInfo.GetDirectories())
            {
                foreach (var file in directory.GetFiles())
                {

                    yield return new MarkdownUnit(file, directory.Name, KnowledgeMode.Instance);
                }
            }

            // TODO:設定ファイルからのLoad
            yield return new SnippetUnit("aim", "AimNext", "Aim");
            yield return new SnippetUnit("Today", DateTime.Now.ToString("d"), "DateTime");
            yield return new SnippetUnit("Now", DateTime.Now.ToString("d"), "DateTime");
            yield return new SnippetUnit("AppData", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            yield return new SnippetUnit("Downloads", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).Replace("Documents", "Downloads"));
            yield return new SnippetUnit("環境変数", "control.exe sysdm.cpl,,3");

            yield return new OptionUnit();
            yield return new ShortcutOptionUnit();
            yield return new EditorUnit(targetPath, string.Empty, OptionMode.Instance);
        }
    }
}
