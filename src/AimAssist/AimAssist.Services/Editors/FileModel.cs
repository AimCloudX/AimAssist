﻿using Common.UI.Editor;
using Library.Options;
using System.IO;
using System.Windows.Controls;

namespace Library.Editors
{
    public class FileModel : TabItem
    {
        private Dictionary<string, string> _extensions = new Dictionary<string, string>()
        {{".md", "markdown" },
            { ".json","json"}
        };

        public FileModel(string filePath)
        {
            FileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            FilePath = filePath;
            monacoEditor = new MonacoEditor();
            monacoEditor.SetOption(EditorOptionService.Option);
            var extension = Path.GetExtension(filePath);
            if(_extensions.TryGetValue(extension, out var language))
            {
                monacoEditor.SetTextAsync(File.ReadAllText(filePath), language);
            }
            else
            {
                monacoEditor.SetTextAsync(File.ReadAllText(filePath));
            }

            Content = monacoEditor;

            // style
            var textblock = new TextBlock();
            textblock.Text = FileNameWithoutExtension;
            Header = textblock;

        }

        public string FileNameWithoutExtension { get; set; }
        public string FilePath { get; set; }

        public MonacoEditor monacoEditor { get; set; }
    }
}
