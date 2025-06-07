namespace AimAssist.Units.ViewProviders
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ViewProviderAttribute : Attribute
    {
        public int Priority { get; set; } = 50;
    }
}
