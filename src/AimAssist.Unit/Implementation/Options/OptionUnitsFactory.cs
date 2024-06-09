using AimAssist.Core.Options;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using System.IO;

namespace AimAssist.Unit.Implementation.Options
{
    public class OptionUnitsFactory : IUnitsFacotry
    {
        public IPickerMode TargetMode => OptionMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            var editorOptionPath = EditorOptionService.OptionPath;
            yield return new OptionUnit("EditorOption", editorOptionPath, "Editor");
        }
    }
}
