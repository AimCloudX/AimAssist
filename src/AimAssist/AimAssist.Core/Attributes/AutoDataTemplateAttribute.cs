using System;

namespace AimAssist.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoDataTemplateAttribute : Attribute
    {
        public Type[] UnitTypes { get; }
        public bool UseDependencyInjection { get; }

        public AutoDataTemplateAttribute(Type unitType, bool useDependencyInjection = false)
            : this(new[] { unitType }, useDependencyInjection)
        {
        }

        public AutoDataTemplateAttribute(Type[] unitTypes, bool useDependencyInjection = false)
        {
            UnitTypes = unitTypes ?? throw new ArgumentNullException(nameof(unitTypes));
            if (unitTypes.Length == 0)
                throw new ArgumentException("At least one unit type must be specified.", nameof(unitTypes));
            
            UseDependencyInjection = useDependencyInjection;
        }

        public Type UnitType => UnitTypes[0];
    }
}
