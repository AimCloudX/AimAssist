namespace AimPicker.Unit.Core.Mode
{
    public interface IPickerMode
    {
        string Name { get; }
        string Prefix { get; }

        string Description { get; }

        bool IsAddUnitLists { get; }
    }
}
