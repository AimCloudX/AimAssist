using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using AimAssist.Unit.Implementation.Options;
using AimAssist.Unit.Implementation.Snippets;
using AimAssist.Unit.Implementation.Web.Bookmarks;
using AimAssist.Unit.Implementation.Web.BookSearch;
using AimAssist.Unit.Implementation.Web.Rss;
using AimAssist.Unit.Implementation.Wiki;
using AimAssist.Unit.Implementation.WorkFlows;

namespace AimAssist.Unit.Implementation.Standard
{
    public class ModeChangeUnitsFacotry : IUnitsFacotry
    {
        public IPickerMode TargetMode => StandardMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return SnippetMode.Instance.ModeChangeUnits;
            yield return RssMode.Instance.ModeChangeUnits;
            yield return KnowledgeMode.Instance.ModeChangeUnits;
            yield return WorkFlowMode.Instance.ModeChangeUnits;
            yield return BookSearchMode.Instance.ModeChangeUnits;
            yield return BookmarkMode.Instance.ModeChangeUnits;
        }
    }
}
