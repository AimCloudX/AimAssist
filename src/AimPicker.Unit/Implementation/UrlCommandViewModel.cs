using AimPicker.Combos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Common.UI
{
    public class UrlCommandViewModel : IUnitViewModel
    {
        private readonly IPreviewFactory previewFactory;

        public string Name { get; }

        public UrlCommandViewModel(string name, string text, IPreviewFactory previewFactory)
        {
            Name = name;
            Text = text;
            this.previewFactory = previewFactory;
            //uiElement = UIElementRepository.GetUIElement(this);

        }

        public string Text { get; }
        private UIElement uiElement;

        public UIElement Create()
        {
            if (uiElement == null)
            {
                uiElement = previewFactory.Create(this);
            }

            return uiElement;
        }
    }
}
