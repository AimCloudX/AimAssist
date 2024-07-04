using AimAssist.Units.Core.Mode;
using AimAssist.Units.Implementation.Knowledge;
using System.IO;

namespace AimAssist.Units.Core.Units
{
    public class MarkdownPathUnit: IUnit
    {
        public MarkdownPathUnit(FileInfo fileInfo, string category)
        {
            FileInfo = fileInfo;
            Category = category;
            FullPath = fileInfo.FullName;
        }

        public string FullPath { get; }

        public FileInfo FileInfo { get; }

        public IMode Mode => KnowledgeMode.Instance;

        public string Name => FileInfo.Name;

        public string Description => FullPath;

        public string Category { get; }
    }
}
