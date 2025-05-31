using AimAssist.Core.Units;
using AimAssist.Units.Implementation.KeyHelp;
using Common.UI.Commands.Shortcus;
using System.IO;

namespace AimAssist.Units.Implementation.Factories
{
    public interface ICheatSheetUnitsFactory
    {
        IEnumerable<IUnit> CreateUnits();
    }

    public class CheatSheetUnitsFactory : ICheatSheetUnitsFactory
    {
        public IEnumerable<IUnit> CreateUnits()
        {
            var cheatSheetDirectory = new DirectoryInfo("Resources/CheatSheet/");
            if (!cheatSheetDirectory.Exists) yield break;

            foreach (var file in cheatSheetDirectory.GetFiles())
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                var text = File.ReadAllText(file.FullName);
                var items = KeySequenceParser.Parse(text, name);
                foreach (var item in items)
                {
                    yield return new KeyHelpUnit(item);
                }
            }
        }
    }
}
