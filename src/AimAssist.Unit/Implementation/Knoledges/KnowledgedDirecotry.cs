using AimAssist.Combos.Mode.Wiki;
using AimAssist.UI.Combos;
using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Wiki;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Implementation.Knoledges
{
    public class KnowledgedDirecotry : IUnitPackage
    {
        public KnowledgedDirecotry(DirectoryInfo directory)
        {
            Directory = directory;
        }

        public BitmapImage Icon =>new BitmapImage();

        public IPickerMode Mode => KnowledgeMode.Instance;

        public string Category => Directory.Parent.Name;

        public string Name => Directory.Name;

        public string Text => Directory.FullName;
        public DirectoryInfo Directory { get; }

        public UIElement GetUiElement()
        {
            var children = this.GetChildren();

            var scrollViewer = new ScrollViewer();

            var files = new List<string>();
            foreach ( var child in children.OfType<KnowledgeUnit>())
            {
                files.Add(child.Text);
            }

            scrollViewer.Content = new MarkdownView(files);
            
            return scrollViewer;
        }

        public IEnumerable<IUnit> GetChildren()
        {
            var direcories = Directory.GetDirectories();
            foreach (var dir in direcories)
            {
                yield return new KnowledgedDirecotry(dir);
            }

            var files = Directory.GetFiles();
            foreach (var file in files)
            {
                yield return new KnowledgeUnit(file);
            }
        }
    }
}
