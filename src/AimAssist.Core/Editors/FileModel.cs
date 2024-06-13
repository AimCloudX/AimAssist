using AimAssist.Core.Options;
using Common.UI.Editor;
using System.IO;
using System.Windows.Controls;

namespace AimAssist.Core.Editors
{
    public class FileModel : TabItem
    {
        public FileModel(string filePath)
        {
            this.FileNameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(filePath);
            this.FilePath = filePath;
            this.monacoEditor = new MonacoEditor();
            this.monacoEditor.SetOption(EditorOptionService.Option);
            this.monacoEditor.SetText(File.ReadAllText(filePath));
            this.Content = this.monacoEditor;

            // style
            var textblock = new TextBlock();
            textblock.Text = this.FileNameWithoutExtension;
            this.Header = textblock;

        }

        public string FileNameWithoutExtension { get; set; }
        public string FilePath { get; set; }

        public MonacoEditor monacoEditor { get; set; }
    }
}
