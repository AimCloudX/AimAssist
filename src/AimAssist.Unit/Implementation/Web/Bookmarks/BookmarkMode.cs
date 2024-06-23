using AimAssist.Unit.Core.Mode;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AimAssist.Unit.Implementation.Web.Bookmarks
{
    public class BookmarkMode : PikcerModeBase
    {
        private BookmarkMode() : base(ModeName) { }

        public const string ModeName = "Bookmark";

        public static BookmarkMode Instance { get; } = new BookmarkMode();

        public override string Description => "ブックマークのPreview";

        public override Control Icon => CreateIcon(PackIconKind.Link);
    }
}
