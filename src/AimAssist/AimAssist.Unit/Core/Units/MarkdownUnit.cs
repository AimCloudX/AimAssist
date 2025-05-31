using System.IO;
using AimAssist.Core.Units;

namespace AimAssist.Units.Core.Units
{
    public class MarkdownUnit: IUnit
    {
        public MarkdownUnit(FileInfo fileInfo, string category, IMode mode)
        {
            FileInfo = fileInfo;
            Category = category;
            FullPath = fileInfo.FullName;
            Mode = mode;
        }

        public string FullPath { get; }

        public FileInfo FileInfo { get; }

        public IMode Mode { get; }

        public string Name => Path.GetFileNameWithoutExtension(FileInfo.Name);

        public string Description => FullPath;

        public string Category { get; }
    }
}
