using System.IO;
using AimAssist.Core.Interfaces;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Options;

namespace AimAssist.Units.UnitFactories
{
    public interface IOptionUnitsFactory
    {
        IEnumerable<IUnit> CreateUnits();
    }

    public class OptionUnitsFactory : IOptionUnitsFactory
    {
        private readonly IWorkItemOptionService workItemOptionService;
        private readonly IEditorOptionService editorOptionService;

        public OptionUnitsFactory(
            IWorkItemOptionService workItemOptionService,
            IEditorOptionService editorOptionService
            )
        {
            this.workItemOptionService = workItemOptionService;
            this.editorOptionService = editorOptionService;
        }

        public IEnumerable<IUnit> CreateUnits()
        {
            var lists = new List<string>();
            lists.Add(workItemOptionService.OptionPath);
            lists.AddRange(workItemOptionService.Option.ItemPaths.Select(x => x.GetActualPath()));
            
            if (File.Exists(editorOptionService.Option.CustomVimKeybindingPath))
            {
                lists.AddRange([editorOptionService.OptionPath, editorOptionService.Option.CustomVimKeybindingPath]);
            }
            else
            {
                lists.AddRange([editorOptionService.OptionPath]);
            }

            // 依存関係があるOptionUnitのみ生成（ShortcutOptionUnitは属性で自動登録）
            yield return new OptionUnit("Option", lists);
        }
    }
}
