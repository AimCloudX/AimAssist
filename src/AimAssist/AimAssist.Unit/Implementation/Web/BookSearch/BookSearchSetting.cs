using AimAssist.Units.Core.Units;
using System.Windows;

namespace AimAssist.Units.Implementation.Web.BookSearch
{
    public class BookSearchSetting : IUnitContent
    {
        public UIElement GetUiElement()
        {
            return new BookSearchControl();
        }
    }
}
