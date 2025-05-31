using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AimAssist.Units.Core.Modes;

namespace AimAssist.Units.Implementation.Web.Bookmarks
{
    public class BookmarkMode : ModeBase
    {
        private BookmarkMode() : base(ModeName) { }

        public const string ModeName = "Bookmark";

        public static BookmarkMode Instance { get; } = new BookmarkMode();

        public override string Description => "ブックマークのPreview";

        public override Control Icon => CreateIcon(PackIconKind.Link);
    }
}
