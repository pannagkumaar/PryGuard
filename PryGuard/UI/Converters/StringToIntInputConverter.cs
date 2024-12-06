using System;
using System.Windows.Data;
using System.Globalization;

namespace PryGuard.UI.Converters;
class StringToIntInputConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue.ToString();
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (int.TryParse(value as string, out int intValue))
        {
            return intValue;
        }
        return 0;
    }
}
