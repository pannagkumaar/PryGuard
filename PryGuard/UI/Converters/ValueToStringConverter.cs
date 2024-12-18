﻿using System;
using System.Windows;
using System.Globalization;
using System.Windows.Data;

namespace PryGuard.UI.Converters;

public class ValueToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return DependencyProperty.UnsetValue;
    }
}
