using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.WorkFlows;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AimAssist.Combos.Mode.WorkFlows
{
    public class WorkFlowUnit : IUnit
    {
        private readonly Func<WorkFlowUnit, UIElement> createUI;

        public string Name { get; }

        public BitmapImage Icon => new BitmapImage();
        public WorkFlowUnit(string name, string text, Func<WorkFlowUnit, UIElement> createUI)
        {
            Name = name;
            Text = text;
            this.createUI = createUI;
        }

        public string Text { get; }
        public IPickerMode Mode => WorkFlowMode.Instance;

        public string Category => string.Empty;

        public UIElement GetUiElement()
        {
            return this.createUI.Invoke(this);
        }
    }
}
