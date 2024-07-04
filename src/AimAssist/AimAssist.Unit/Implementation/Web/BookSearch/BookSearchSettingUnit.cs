using AimAssist.Units.Core.Mode;
using AimAssist.Units.Core.Units;

namespace AimAssist.Units.Implementation.Web.BookSearch
{
    public class BookSearchSettingUnit : IUnit
    {
        public IMode Mode => BookSearchMode.Instance;

        public string Name => "BookSearch";

        public string Description => "";

        public string Category => "";
    }
}
