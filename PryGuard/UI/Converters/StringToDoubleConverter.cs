using System;
using System.Windows;
using System.Globalization;
using System.Windows.Data;

namespace PryGuard.UI.Converters;
public class StringToDoubleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return 0;

        return double.Parse(value.ToString(), CultureInfo.InvariantCulture);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}
