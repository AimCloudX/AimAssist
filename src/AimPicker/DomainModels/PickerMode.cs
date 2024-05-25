namespace AimPicker.Domain
{
    public interface IPickerMode
    {
        string Name { get; }
    }

    public abstract class PickerMode : IPickerMode
    {
        protected PickerMode(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public override bool Equals(object? obj)
        {
            if(obj is IPickerMode pickerMode)
            {
                return this.Name.Equals(pickerMode.Name, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class SnippetMode : PickerMode
    {
        private SnippetMode(): base(ModeName) {}

        public const string ModeName = "Snippet";

        public static SnippetMode Instance { get; } = new SnippetMode();
    }
    public class WorkFlowMode : PickerMode
    {
        private WorkFlowMode(): base(ModeName) {}

        public const string ModeName = "WorkFlow";

        public static WorkFlowMode Instance { get; } = new WorkFlowMode();
    }

    public class CalculationMode : PickerMode
    {
        private CalculationMode(): base(ModeName) {}

        public const string ModeName = "Calculation";

        public static CalculationMode Instance { get; } = new CalculationMode();
    }
    public class UrlMode : PickerMode
    {
        private UrlMode(): base(ModeName) {}

        public const string ModeName = "URL";

        public static UrlMode Instance { get; } = new UrlMode();
    }
    public class BookSearchMode : PickerMode
    {
        private BookSearchMode(): base(ModeName) {}

        public const string ModeName = "BookSearch";

        public static BookSearchMode Instance { get; } = new BookSearchMode();
    }
}
