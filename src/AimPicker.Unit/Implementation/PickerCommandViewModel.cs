using System.Windows;

namespace AimPicker.Combos.Mode
{
    public class PickerCommandViewModel : IUnitViewModel
    {
        public string Name { get; }

        public PickerCommandViewModel(string name, string text, IPreviewFactory factory)
        {
            Name = name;
            Text = text;
            Factory = factory;
            // TODO:Keepするかの判断を移動させる
            //if (factory.IsKeepUiElement)
            //{
            //    //uiElement = UIElementRepository.GetUIElement(this);
            //}
            //else
            //{
                uiElement = factory.Create(this);
            //}
        }

        public string Text { get; }
        public IPreviewFactory Factory { get; }

        private UIElement uiElement;

        public UIElement Create()
        {
            if (uiElement == null)
            {
                uiElement = Factory.Create(this);
            }

            return uiElement;
        }
    }
}
