using AimAssist.Unit.Core;
using AimAssist.Unit.Core.Mode;
using System.ComponentModel;
using System.Globalization;

namespace AimAssist.UI.Units
{
    public class CustomGroupDescription : GroupDescription
    {
        public override object GroupNameFromItem(object item, int level, CultureInfo culture)
        {
            if (item is IUnit yourItem)
            {
                // 要素ごとに異なるプロパティでグループ化する
                if (yourItem is ModeChangeUnit)
                {
                    return yourItem.Category;
                }
                else
                {
                    return yourItem.Mode.Name;
                }
            }

            return string.Empty;
        }

        public override bool NamesMatch(object groupName, object itemName)
        {
            return Equals(groupName, itemName);
        }
    }
}

