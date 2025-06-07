using System;

namespace AimAssist.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoDataTemplateAttribute : Attribute
    {
        public Type UnitType { get; }

        public AutoDataTemplateAttribute(Type unitType)
        {
            UnitType = unitType ?? throw new ArgumentNullException(nameof(unitType));
        }
    }
}
