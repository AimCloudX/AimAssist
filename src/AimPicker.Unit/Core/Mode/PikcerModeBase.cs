namespace AimPicker.Unit.Core.Mode
{

    public abstract class PikcerModeBase : IPickerMode
    {
        protected PikcerModeBase(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public virtual string Prefix => string.Empty;
        public virtual bool IsAddUnitLists => true;

        public virtual string Description => string.Empty;

        public override bool Equals(object? obj)
        {
            if (obj is IPickerMode pickerMode)
            {
                return Name.Equals(pickerMode.Name, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
