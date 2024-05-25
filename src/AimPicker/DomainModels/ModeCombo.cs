using AimPicker.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimPicker.DomainModels
{
    public class ModeCombo : ICombo
    {
        public string Name { get; }

        public string Text { get; }
        public IPickerMode PickerMode { get; }

        public ModeCombo(IPickerMode pickerMode)
        {
            Name = pickerMode.Name;
            Text = pickerMode.Prefix;
            PickerMode = pickerMode;
        }
    }
}
