using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Knowledge;
using AimAssist.Unit.Implementation.Snippets;
using AimAssist.Unit.Implementation.Web.Bookmarks;
using AimAssist.Unit.Implementation.Web.BookSearch;
using AimAssist.Unit.Implementation.Web.Rss;
using AimAssist.Unit.Implementation.WorkTools;

namespace AimAssist.Unit.Implementation.Standard
{
    public class ModeChangeUnitsFacotry : IUnitsFacotry
    {
        public IPickerMode TargetMode => StandardMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return WorkToolsMode.Instance.ModeChangeUnit;
            yield return BookSearchMode.Instance.ModeChangeUnit;
            yield return KnowledgeMode.Instance.ModeChangeUnit;
            yield return RssMode.Instance.ModeChangeUnit;
            yield return BookmarkMode.Instance.ModeChangeUnit;
            yield return SnippetMode.Instance.ModeChangeUnit;
        }
    }
}
