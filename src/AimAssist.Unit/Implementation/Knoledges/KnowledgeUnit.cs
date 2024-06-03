using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Wiki;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Combos.Mode.Wiki
{
    public class KnowledgeUnit : IUnit
    {
        private readonly FileInfo fileInfo;

        public string Name =>
                 System.IO.Path.GetFileNameWithoutExtension(fileInfo.Name);

        public KnowledgeUnit(FileInfo fileInfo)
        {
            this.fileInfo = fileInfo;
        }

        public string Text => this.Path;

        public string Path => fileInfo.FullName;

        public MarkdownPreviewFactory PreviewFactory => new MarkdownPreviewFactory();

        public IPickerMode Mode => KnowledgeMode.Instance;

        public string Category => string.Empty;

        public BitmapImage Icon => new BitmapImage();

        public UIElement GetUiElement()
        {
            return new MarkdownPreviewFactory().Create(this.Path);
        }
    }
}
