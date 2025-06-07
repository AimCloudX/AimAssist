using System;
using System.Windows;
using AimAssist.Core.Units;
using AimAssist.Units.Implementation.Web.MindMeister;
using Common.UI.WebUI;

namespace AimAssist.UI.UnitContentsView.ViewProviders
{
    [ViewProvider(Priority = 85)]
    public class MindMeisterViewProvider : IViewProvider
    {
        public int Priority => 85;

        public bool CanProvideView(Type unitType) => 
            unitType == typeof(MindMeisterUnit) || 
            unitType == typeof(MindMeisterItemUnit);

        public UIElement CreateView(IUnit unit, IServiceProvider serviceProvider)
        {
            return unit switch
            {
                MindMeisterUnit mindMeisterUnit => new MindMeisterViewControl(mindMeisterUnit),
                MindMeisterItemUnit itemUnit => new WebViewControl(itemUnit.SearchUrl, itemUnit.Name),
                _ => null
            };
        }
    }
}
