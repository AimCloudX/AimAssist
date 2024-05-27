namespace AimPicker.Unit.Core.Mode
{

    public abstract class PikcerModeBase : IPickerMode
    {
        protected PikcerModeBase(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public abstract string Prefix { get; }

        public virtual string Description { get; set; }

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
