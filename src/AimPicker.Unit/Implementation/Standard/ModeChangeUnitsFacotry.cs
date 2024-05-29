using AimPicker.Unit.Core;
using AimPicker.Unit.Core.Mode;
using AimPicker.Unit.Implementation.Snippets;
using AimPicker.Unit.Implementation.Web.Bookmarks;
using AimPicker.Unit.Implementation.Web.BookSearch;
using AimPicker.Unit.Implementation.Wiki;
using AimPicker.Unit.Implementation.WorkFlows;

namespace AimPicker.Unit.Implementation.Standard
{
    public class ModeChangeUnitsFacotry : IUnitsFacotry
    {
        public IPickerMode TargetMode => StandardMode.Instance;

        public bool IsShowInStnadard => true;

        public async IAsyncEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return SnippetMode.Instance.ModeChangeUnits;
            yield return KnowledgeMode.Instance.ModeChangeUnits;
            yield return WorkFlowMode.Instance.ModeChangeUnits;
            yield return BookSearchMode.Instance.ModeChangeUnits;
            yield return BookmarkMode.Instance.ModeChangeUnits;
        }
    }
}
