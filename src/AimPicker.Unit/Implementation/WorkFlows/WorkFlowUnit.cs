using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.WorkFlows;
using System.Windows;

namespace AimPicker.Combos.Mode.WorkFlows
{
    public class WorkFlowUnit : IUnit
    {
        private readonly Func<WorkFlowUnit, UIElement> createUI;

        public string Name { get; }

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
