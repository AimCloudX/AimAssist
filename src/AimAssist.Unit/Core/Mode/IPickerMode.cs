namespace AimAssist.Unit.Core.Mode
{
    public interface IPickerMode
    {
        string Name { get; }
        string Prefix { get; }

        string Description { get; }

        bool IsApplyFiter { get; }
    }
}
