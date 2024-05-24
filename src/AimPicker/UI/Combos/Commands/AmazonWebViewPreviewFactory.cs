using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace AimPicker.UI.Combos.Commands
{
    public class AmazonWebViewPreviewFactory: IPreviewFactory
    {
        public UIElement Create(IComboViewModel combo)
        {
            return new AmazonWebViewControl(combo.Description);
        }
    }
}