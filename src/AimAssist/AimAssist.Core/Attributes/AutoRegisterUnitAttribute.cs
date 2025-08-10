using System;

namespace AimAssist.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoRegisterUnitAttribute : Attribute
    {
        public string Category { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public string Description { get; set; } = string.Empty;

        public AutoRegisterUnitAttribute()
        {
        }

        public AutoRegisterUnitAttribute(string category)
        {
            Category = category;
        }
    }
}
