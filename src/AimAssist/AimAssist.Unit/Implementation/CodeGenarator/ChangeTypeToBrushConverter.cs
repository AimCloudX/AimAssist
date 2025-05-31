using System.Drawing;
using System.Globalization;
using System.Windows.Data;
using DiffPlex.DiffBuilder.Model;

namespace AimAssist.Units.Implementation.CodeGenarator
{
public class ChangeTypeToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ChangeType type)
            {
                switch (type)
                {
                    case ChangeType.Deleted:
                        return Brushes.LightCoral;
                    case ChangeType.Inserted:
                        return Brushes.LightGreen;
                    case ChangeType.Modified:
                        return Brushes.LightBlue;
                    default:
                        return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
