using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BiliLive.Views.MainWindow.Converter;

public class ProgressToWidthConverter : IValueConverter
{
    public static readonly ProgressToWidthConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double progress)
        {
            // Assuming the notification width is around 300px, calculate proportional width
            // The progress bar should scale with the notification content
            return progress / 100.0 * 300.0;
        }
        return 0.0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

