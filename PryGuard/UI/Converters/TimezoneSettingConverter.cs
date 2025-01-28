namespace PryGuard.UI.Converters
{
    using PryGuard.Core.Browser.Model.Configs;
    using System;
    using System.Globalization;
    using System.Windows.Data;

    public class TimezoneSettingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimezoneSetting timezoneSetting)
            {
                return timezoneSetting.StringPresent;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return new TimezoneSetting
                {
                    CustomTimezone = stringValue,
                    HideTimezone = false
                };
            }

            return null;
        }
    }
}
