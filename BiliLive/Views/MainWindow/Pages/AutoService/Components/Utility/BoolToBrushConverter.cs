using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace BiliLive.Views.MainWindow.Pages.AutoService.Components.Utility
{
    public class BoolToBrushConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var brushStrings = (parameter as string)?.Split(',');
            if (value is not bool isTrue || brushStrings?.Length != 2) return AvaloniaProperty.UnsetValue;
            var colorString = isTrue ? brushStrings[0] : brushStrings[1];
                
            return Color.TryParse(colorString, out Color color) ? new SolidColorBrush(color) : AvaloniaProperty.UnsetValue;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            //无此方法
            return null;
        }
    }
}