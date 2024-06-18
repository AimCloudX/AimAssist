using AimAssist.Combos.Mode.Wiki;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Knowledge;
using System.IO;

namespace AimAssist.Unit.Implementation.Knoledges
{
    public class KnowledgeUnitsFactory : IUnitsFacotry
    {
        public bool IsShowInStnadard => true;

        public IPickerMode TargetMode => KnowledgeMode.Instance;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            var dictInfo = new DirectoryInfo("Resources/Knowledge/");
            foreach (var directory in dictInfo.GetDirectories())
            {
                yield return new KnowledgedDirecotry(directory);
            }

            foreach (var file in dictInfo.GetFiles())
            {
                yield return new KnowledgeUnit(file);
            }
        }
    }
}
