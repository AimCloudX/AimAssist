using AimAssist.Units.Core;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.ApplicationLog;
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
            yield return new UrlUnit(WorkToolsMode.Instance, "ChatGPT", "https://chatgpt.com/");
            yield return new UrlUnit(WorkToolsMode.Instance, "Claude", "https://claude.ai/");
            yield return new TranscriptionUnit();
            yield return new BookSearchSettingUnit();
            yield return new RssSettingUnit();
            yield return new AppLogUnit();

            var dictInfo = new DirectoryInfo("Resources/Knowledge/");
            foreach (var file in dictInfo.GetFiles())
            {
                yield return new MarkdownPathUnit(file, string.Empty);
            }

            foreach (var directory in dictInfo.GetDirectories())
            {
                foreach (var file in directory.GetFiles())
                {

                    yield return new MarkdownPathUnit(file, directory.Name);
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
        }
    }
}
