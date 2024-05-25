using AimPicker.UI.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace AimPicker.UI.Combos.Commands
{
    public class UrlCommandViewModel : IComboViewModel
    {
        private readonly IPreviewFactory previewFactory;

        public string Name { get; }

        public UrlCommandViewModel(string name, string text, IPreviewFactory previewFactory)
        {
            Name = name;
            this.Text = text;
            this.previewFactory = previewFactory;
            uiElement = UIElementRepository.GetUIElement(this);

        }

        public string Text { get; }
        private UIElement uiElement;

        public UIElement Create()
        {
            if(uiElement == null)
            {
                uiElement = this.previewFactory.Create(this);
            }

           return  uiElement;
        }
    }
}
