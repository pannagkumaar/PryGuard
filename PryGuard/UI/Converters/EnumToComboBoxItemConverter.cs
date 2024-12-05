using System;
using System.Globalization;
using System.Windows.Data;

namespace PryGuard.UI.Converters
{
    public class EnumToComboBoxItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !value.GetType().IsEnum)
                return null;

            var enumValues = Enum.GetValues(value.GetType());
            var comboBoxItems = new System.Collections.Generic.List<string>();

            foreach (var enumValue in enumValues)
            {
                comboBoxItems.Add(enumValue.ToString());
            }

            return comboBoxItems;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
