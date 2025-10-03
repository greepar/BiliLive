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
            return b ? Brush.Parse("#26AE36") : Brushes.Red; // true → 不透明, false → 透明
        return Brush.Parse("#D3DA00");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}