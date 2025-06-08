using System;

namespace AimAssist.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoDataTemplateAttribute : Attribute
    {
        public Type[] UnitTypes { get; }
        public AutoDataTemplateAttribute(Type unitType)
            : this([unitType])
        {
        }

        public AutoDataTemplateAttribute(Type[] unitTypes)
        {
            UnitTypes = unitTypes ?? throw new ArgumentNullException(nameof(unitTypes));
            if (unitTypes.Length == 0)
                throw new ArgumentException("At least one unit type must be specified.", nameof(unitTypes));
        }

        public Type UnitType => UnitTypes[0];
    }
}
