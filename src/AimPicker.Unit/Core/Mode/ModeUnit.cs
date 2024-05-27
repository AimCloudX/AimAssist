using AimPicker.Unit.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AimPicker.Unit.Core.Mode
{
    public class ModeUnit : IUnit
    {
        public string Name { get; }

        public string Text { get; }
        public IPickerMode PickerMode { get; }

        public UIElement PreviewUI => throw new NotImplementedException();

        public ModeUnit(IPickerMode pickerMode)
        {
            Name = pickerMode.Name;
            Text = pickerMode.Prefix;
            PickerMode = pickerMode;
        }
    }
}
