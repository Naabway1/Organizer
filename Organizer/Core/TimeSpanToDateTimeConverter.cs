using System.Globalization;
using System;
using System.Windows.Data;

namespace Organizer.Core
{
    public class TimeSpanToDateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan time)
            {
                return DateTime.Today.Add(time);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dt)
            {
                return dt.TimeOfDay;
            }

            return null;
        }
    }
}
