using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.WorkFlows;
using System.Windows;

namespace AimPicker.Combos.Mode.WorkFlows
{
    public class WorkFlowCombo : IUnit
    {
        public string Name { get; }

        public WorkFlowCombo(string name, string text, IPreviewFactory previewFactory)
        {
            Name = name;
            Text = text;
            this.previewFactory = previewFactory;
        }

        public string Text { get; }
        private IPreviewFactory previewFactory { get; }

        public IPickerMode Mode => WorkFlowMode.Instance;

        public string Category => string.Empty;

        public UIElement GetUiElement()
        {
            return this.previewFactory.Create(this);
        }
    }
}
