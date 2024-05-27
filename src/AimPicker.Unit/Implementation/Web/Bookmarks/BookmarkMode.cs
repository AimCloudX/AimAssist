using AimPicker.Unit.Core.Mode;

namespace AimPicker.Unit.Implementation.Web.Bookmarks
{
    public class BookmarkMode : PikcerModeBase
    {
        private BookmarkMode() : base(ModeName) { }

        public const string ModeName = "Bookmark";

        public static BookmarkMode Instance { get; } = new BookmarkMode();

        public override string Prefix => "bm ";
        public override string Description => "ブックマークのPreview";
    }
}
