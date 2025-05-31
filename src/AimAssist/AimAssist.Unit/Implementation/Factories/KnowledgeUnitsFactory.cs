using AimAssist.Core.Units;
using AimAssist.Units.Core.Units;
using System.IO;
using AimAssist.Units.Implementation.Knowledges;

namespace AimAssist.Units.Implementation.Factories
{
    public interface IKnowledgeUnitsFactory
    {
        IEnumerable<IUnit> CreateUnits();
    }

    public class KnowledgeUnitsFactory : IKnowledgeUnitsFactory
    {
        public IEnumerable<IUnit> CreateUnits()
        {
            var dictInfo = new DirectoryInfo("Resources/Knowledge/");
            if (!dictInfo.Exists) yield break;

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
        }
    }
}
