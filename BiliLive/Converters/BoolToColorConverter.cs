using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace BiliLive.Converters;

public class BoolToColorConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? Brushes.Green : Brushes.Red; // true → 不透明, false → 透明
        return Brushes.Yellow;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return false;
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}