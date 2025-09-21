using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BiliLive.Views.MainWindow.Pages.AutoService.Components.Utility
{
    public class NegateDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d)
                return -d;
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}