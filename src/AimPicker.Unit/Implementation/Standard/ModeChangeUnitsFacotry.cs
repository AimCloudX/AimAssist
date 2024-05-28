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

        public IEnumerable<IUnit> GetUnits(UnitsFactoryParameter pamater)
        {
            yield return new ModeChangeUnit(SnippetMode.Instance);
            yield return new ModeChangeUnit(KnowledgeMode.Instance);
            yield return new ModeChangeUnit(WorkFlowMode.Instance);
            yield return new ModeChangeUnit(BookSearchMode.Instance);
            yield return new ModeChangeUnit(BookmarkMode.Instance);
        }
    }
}
