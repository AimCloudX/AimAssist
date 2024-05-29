using AimPicker.Unit.Core.Mode;

namespace AimPicker.Unit.Implementation.Web.BookSearch
{
    public class BookSearchMode : PikcerModeBase
    {
        private BookSearchMode() : base(ModeName) { }

        public const string ModeName = "BookSearch";

        public static BookSearchMode Instance { get; } = new BookSearchMode();

        public override string Prefix => "bs ";
        public override string Description => "入力されたテキストを元に Google Books APIを使用して本を探して、ISBN10からamazonのリンクを作成して表示";

        public override bool IsApplyFiter => false; 
    }
}
