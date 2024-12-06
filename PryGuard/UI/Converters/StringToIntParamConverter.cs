using System;
using System.Windows.Data;
using System.Globalization;

namespace PryGuard.UI.Converters;

public class StringToIntParamConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int currency)
        {
            return currency == int.Parse(parameter.ToString());
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked)
        {
            if (isChecked)
            {
                return parameter;
            }
        }
        return Binding.DoNothing;
    }
}