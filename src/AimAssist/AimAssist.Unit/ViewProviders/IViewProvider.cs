using System.Windows;
using AimAssist.Core.Units;

namespace AimAssist.Units.ViewProviders
{
    public interface IViewProvider
    {
        bool CanProvideView(Type unitType);
        UIElement CreateView(IUnit unit, IServiceProvider serviceProvider);
        int Priority { get; }
    }
}
