using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.WorkTools;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Combos.Mode.WorkTools
{
    public class WorkToolUnit : IUnit
    {
        private readonly Func<WorkToolUnit, UIElement> createUI;

        public string Name { get; }

        public BitmapImage Icon => new BitmapImage();
        public WorkToolUnit(string name, string text, Func<WorkToolUnit, UIElement> createUI)
        {
            Name = name;
            Text = text;
            this.createUI = createUI;
        }

        public string Text { get; }
        public IMode Mode => WorkToolsMode.Instance;

        public string Category => string.Empty;

        public UIElement GetUiElement()
        {
            return this.createUI.Invoke(this);
        }
    }
}
