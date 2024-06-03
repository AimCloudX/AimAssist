using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AimAssist.Windows.PickerWindows
{
    using AimAssist.Unit.Core;
    using AimAssist.Unit.Core.Mode;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;

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
            return object.Equals(groupName, itemName);
        }
    }
}

