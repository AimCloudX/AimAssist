using System;

namespace AimAssist.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModeDisplayOrderAttribute : Attribute
    {
        public int Order { get; }
        
        public ModeDisplayOrderAttribute(int order)
        {
            Order = order;
        }
    }
}
