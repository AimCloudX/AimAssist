using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Options;
using Library.Options;
using System.IO;
using AimAssist.Core.Interfaces;

namespace AimAssist.Units.Implementation.Factories
{
    public interface IOptionUnitsFactory
    {
        IEnumerable<IUnit> CreateUnits();
    }

    public class OptionUnitsFactory : IOptionUnitsFactory
    {
        private readonly IWorkItemOptionService _workItemOptionService;
        private readonly IEditorOptionService _editorOptionService;
        private readonly ISnippetOptionService _snippetOptionService;

        public OptionUnitsFactory(
            IWorkItemOptionService workItemOptionService,
            IEditorOptionService editorOptionService,
            ISnippetOptionService snippetOptionService)
        {
            _workItemOptionService = workItemOptionService;
            _editorOptionService = editorOptionService;
            _snippetOptionService = snippetOptionService;
        }

        public IEnumerable<IUnit> CreateUnits()
        {
            var lists = new List<string>();
            lists.Add(_workItemOptionService.OptionPath);
            lists.AddRange(_workItemOptionService.Option.ItemPaths.Select(x => x.GetActualPath()));
            
            if (File.Exists(_editorOptionService.Option.CustomVimKeybindingPath))
            {
                lists.AddRange([_editorOptionService.OptionPath, _editorOptionService.Option.CustomVimKeybindingPath]);
            }
            else
            {
                lists.AddRange([_editorOptionService.OptionPath]);
            }

            lists.AddRange([_snippetOptionService.OptionPath]);
            lists.AddRange(_snippetOptionService.Option.ItemPaths.Select(x => x.GetActualPath()));
            
            yield return new OptionUnit("Option", lists);
            yield return new ShortcutOptionUnit();
        }
    }
}
