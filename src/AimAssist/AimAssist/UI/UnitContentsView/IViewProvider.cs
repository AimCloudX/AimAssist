using System;
using System.Windows;
using AimAssist.Core.Units;

namespace AimAssist.UI.UnitContentsView
{
    public interface IViewProvider
    {
        bool CanProvideView(Type unitType);
        UIElement CreateView(IUnit unit, IServiceProvider serviceProvider);
        int Priority { get; }
    }
}
