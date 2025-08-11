using System.Windows;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.ExcelStamp;

namespace AimAssist.Units.ViewProviders.Providers
{
    [ViewProvider(Priority = 90)]
    public class ExcelStampViewProvider : IViewProvider
    {
        public int Priority => 90;

        public bool CanProvideView(Type unitType) =>
            unitType == typeof(ExcelStampUnit);

        public UIElement CreateView(IUnit unit, IServiceProvider serviceProvider)
        {
            return new ExcelStampView();
        }
    }
}