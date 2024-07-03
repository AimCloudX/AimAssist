using AimAssist.Units.Core;
using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;
using AimAssist.Units.Implementation.Knowledge;
using System.IO;
using System.Windows.Media.Imaging;

namespace AimAssist.Units.Implementation.Knoledges
{
    public class KnowledgeUnitsFactory : IUnitsFacotry
    {
        public bool IsShowInStnadard => true;

        public IMode TargetMode => KnowledgeMode.Instance;

        public async IAsyncEnumerable<IUnit> GetUnits()
        {
            var dictInfo = new DirectoryInfo("Resources/Knowledge/");
            foreach (var file in dictInfo.GetFiles())
            {
                yield return new Unit(KnowledgeMode.Instance, file.Name, new MarkdownPath(file));
            }

            foreach (var directory in dictInfo.GetDirectories())
            {
                foreach (var file in directory.GetFiles())
                {

                    yield return new Unit(KnowledgeMode.Instance, file.Name, string.Empty, directory.Name, new BitmapImage(), new MarkdownPath(file));
                }
            }

        }
    }
}
