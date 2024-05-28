using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;

namespace AimPicker.Unit.Implementation.Web.Urls
{
    public class UrlUnitsFacotry : IUnitsFacotry
    {
        public IPickerMode TargetMode => UrlMode.Instance;

        public bool IsShowInStnadard => false;

        public IEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return new UrlUnit("URL Preview", pamater.InputText, new WebViewPreviewFactory());
        }
    }
}
