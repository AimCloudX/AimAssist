using AimAssist.Services.Markdown;
using AimAssist.Units.Core;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.KeyHelp;
using AimAssist.Units.Implementation.Knowledge;
using AimAssist.Units.Implementation.Options;
using AimAssist.Units.Implementation.Snippets;
using AimAssist.Units.Implementation.Web.Rss;
using AimAssist.Units.Implementation.WorkTools;
using Common.UI.Commands.Shortcus;
using Library.Options;
using System.IO;

namespace AimAssist.Units.Implementation
{
    public class UnitsFactory : IUnitsFacotry
    {
        public IEnumerable<IUnit> GetUnits()
        {
            foreach (var workItemPath in WorkItemOptionService.Option.ItemPaths)
            {
                var links = new MarkdownService().GetLinks(workItemPath.GetActualPath());

                foreach (var link in links)
                {
                    yield return new UrlUnit(WorkToolsMode.Instance, link.Text, link.Url, link.ContainingHeader);
                }
            }

            yield return new TranscriptionUnit();
            yield return new RssSettingUnit();

            //yield return new AppLogUnit();

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

            var parser = new SnippetParser();
            foreach (var path in SnippetOptionServce.Option.ItemPaths)
            {
                var snippets = parser.ParseMarkdownFile(path.GetActualPath());
                foreach (var snippet in snippets)
                {
                    yield return new SnippetModelUnit(snippet);
                }
            }

            var cheatSheetDirectory = new DirectoryInfo("Resources/CheatSheet/");
            foreach (var file in cheatSheetDirectory.GetFiles())
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                var text = File.ReadAllText(file.FullName);
                var items  =  KeySequenceParser.Parse(text, name);
                foreach(var item in items)
                {
                    yield return new KeyHelpUnit(item);
                }
            }

            var lists = new List<string>();
            lists.Add(WorkItemOptionService.OptionPath);
            lists.AddRange(WorkItemOptionService.Option.ItemPaths.Select(x => x.GetActualPath()));
            if (File.Exists(EditorOptionService.Option.CustomVimKeybindingPath))
            {
                lists.AddRange([EditorOptionService.OptionPath, EditorOptionService.Option.CustomVimKeybindingPath]);
            }
            else
            {
                lists.AddRange([EditorOptionService.OptionPath]);
            }

            lists.AddRange([SnippetOptionServce.OptionPath]);
            lists.AddRange(SnippetOptionServce.Option.ItemPaths.Select(x => x.GetActualPath()));
            yield return new OptionUnit("Option", lists);

            yield return new ShortcutOptionUnit();
        }
    }
}
