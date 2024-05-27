using AimPicker.Unit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            PreviewFactory = previewFactory;
        }

        public string Text { get; }
        public IPreviewFactory PreviewFactory { get; }

        public UIElement PreviewUI => throw new NotImplementedException();
    }
}
