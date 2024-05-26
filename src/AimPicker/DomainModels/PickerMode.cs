namespace AimPicker.Domain
{
    public interface IPickerMode
    {
        string Name { get; }
        string Prefix { get; }

        string Description { get; }
    }

    public abstract class PickerMode : IPickerMode
    {
        protected PickerMode(string name)
        {
            this.Name = name;
        }

        public string Name { get; }

        public abstract string Prefix { get; }

        public virtual string Description { get; set; }

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

    public class NormalMode : PickerMode
    {
        private NormalMode(): base(ModeName) {}

        public const string ModeName = "AimPicker";

        public static NormalMode Instance { get; } = new NormalMode();

        public override string Prefix => "";

        public override string Description => "モード選択";
    }

    public class SnippetMode : PickerMode
    {
        private SnippetMode(): base(ModeName) {}

        public const string ModeName = "Snippet";

        public static SnippetMode Instance { get; } = new SnippetMode();

        public override string Prefix => "sn ";

        public override string Description => "スニペットモード";
    }
    public class WorkFlowMode : PickerMode
    {
        private WorkFlowMode(): base(ModeName) {}

        public const string ModeName = "WorkFlow";

        public static WorkFlowMode Instance { get; } = new WorkFlowMode();

        public override string Prefix => ">";
        public override string Description => "登録されたWorkFlowを表示";
    }

    public class CalculationMode : PickerMode
    {
        private CalculationMode(): base(ModeName) {}

        public const string ModeName = "Calculation";

        public static CalculationMode Instance { get; } = new CalculationMode();

        public override string Prefix => "=";
    }
    public class UrlMode : PickerMode
    {
        private UrlMode(): base(ModeName) {}

        public const string ModeName = "URL";

        public static UrlMode Instance { get; } = new UrlMode();

        public override string Prefix => "https://";
    }
    public class BookSearchMode : PickerMode
    {
        private BookSearchMode(): base(ModeName) {}

        public const string ModeName = "BookSearch";

        public static BookSearchMode Instance { get; } = new BookSearchMode();

        public override string Prefix => "bs ";
        public override string Description => "入力されたテキストを元に Google Books APIを使用して本を探して、ISBN10からamazonのリンクを作成して表示";
    }
    public class BookmarkMode : PickerMode
    {
        private BookmarkMode(): base(ModeName) {}

        public const string ModeName = "Bookmark";

        public static BookmarkMode Instance { get; } = new BookmarkMode();

        public override string Prefix => "bm ";
        public override string Description => "ブックマークのPreview";
    }
    public class WikiMode : PickerMode
    {
        private WikiMode(): base(ModeName) {}

        public const string ModeName = "Wiki";

        public static WikiMode Instance { get; } = new WikiMode();

        public override string Prefix => "wiki ";
        public override string Description => "AimPicker開発のwiki";
    }
}
