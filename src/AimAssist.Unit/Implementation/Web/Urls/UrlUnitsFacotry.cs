﻿using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;

namespace AimAssist.Unit.Implementation.Web.Urls
{
    public class UrlUnitsFacotry : IUnitsFacotry
    {
        public IPickerMode TargetMode => UrlMode.Instance;

        public bool IsShowInStnadard => false;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return new UrlUnit("URL Preview", pamater.InputText);
        }
    }
}