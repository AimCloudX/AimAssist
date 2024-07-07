using AimAssist.Units.Core.Mode;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;

namespace AimAssist.Units.Implementation.Web.BookSearch
{
    public class BookSearchMode : ModeBase
    {
        private BookSearchMode() : base(ModeName) { }

        public const string ModeName = "BookSearch";

        public static BookSearchMode Instance { get; } = new BookSearchMode();

        public override Control Icon => CreateIcon(PackIconKind.BookAlphabet);

        public override string Description => "入力されたテキストを元に Google Books APIを使用して本を探して、ISBN10からamazonのリンクを作成して表示";
    }
}
