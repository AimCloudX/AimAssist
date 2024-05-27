using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using AimPicker.Combos;
using AimPicker.UI.Combos.Commands;

namespace Common.UI.Amazon
{
    public class AmazonWebViewPreviewFactory : IPreviewFactory
    {
        public bool IsKeepUiElement => true;

        public UIElement Create(IUnitViewModel combo)
        {
            return new AmazonWebViewControl(combo.Text);
        }
    }
}