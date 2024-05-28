using AimPicker.Combos.Mode.Wiki;
using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Wiki;
using System.IO;

namespace AimPicker.Unit.Implementation.Knoledges
{
    public class KnowledgeUnitsFactory : IUnitsFacotry
    {
        public bool IsShowInStnadard => true;

        public IPickerMode TargetMode => KnowledgeMode.Instance;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            var dictInfo = new DirectoryInfo("Resources/Knowledge/");
            foreach (var file in dictInfo.GetFiles())
            {
                var fileName = Path.GetFileNameWithoutExtension(file.Name);
                yield return new KnowledgeUnit(fileName, file.FullName);
            }
        }
    }
}
