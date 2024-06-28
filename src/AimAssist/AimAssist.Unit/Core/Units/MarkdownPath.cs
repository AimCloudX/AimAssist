using System.IO;

namespace AimAssist.Units.Core.Units
{
    public class MarkdownPath: IUnitContent
    {
        public MarkdownPath(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
            FullPath = fileInfo.FullName;
        }

        public string FullPath { get; }

        public FileInfo FileInfo { get; }
    }
}
