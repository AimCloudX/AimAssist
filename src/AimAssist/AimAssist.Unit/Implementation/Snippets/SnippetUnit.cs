using AimAssist.Units.Core.Units;
using System.Windows.Media.Imaging;

namespace AimAssist.Units.Implementation.Snippets
{
    public class SnippetUnit : Unit
    {
        private SnippetUnit(string category, SnippetModel model) 
            : base(SnippetMode.Instance, model.Name, model.Code, model.Category, new BitmapImage(), model)
        {
            this.Model = model;
        }

        public static SnippetUnit Create(string name, string code, string category = "")
        {
            return new SnippetUnit(category, new SnippetModel(name, code, category));
        }

        public SnippetModel Model { get; }
    }
}
