using AttendeeAnalyzer.Meetup.Models;
using System;
using Windows.UI.Xaml.Data;

namespace AttendeeRegister.Converters
{
    public class RSVPConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value as RSVP;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value;
        }
    }
}
