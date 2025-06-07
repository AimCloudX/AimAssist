using System;

namespace AimAssist.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoDataTemplateAttribute : Attribute
    {
        public Type UnitType { get; }
        public bool UseDependencyInjection { get; }

        public AutoDataTemplateAttribute(Type unitType, bool useDependencyInjection = false)
        {
            UnitType = unitType ?? throw new ArgumentNullException(nameof(unitType));
            UseDependencyInjection = useDependencyInjection;
        }
    }
}
