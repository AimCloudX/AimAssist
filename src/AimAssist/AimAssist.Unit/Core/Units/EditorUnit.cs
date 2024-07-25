using AimAssist.Units.Core.Mode;
using System.IO;

namespace AimAssist.Units.Core.Units
{
    public class EditorUnit : IUnit
    {
        public EditorUnit(string filePath, string category, IMode mode)
        {
            Name = Path.GetFileNameWithoutExtension(filePath);
            Category = category;
            FullPath = filePath;
            Mode = mode;
        }

        public string FullPath { get; }

        public IMode Mode { get; }

        public string Name { get; }

        public string Description => FullPath;

        public string Category { get; }
    }

}
