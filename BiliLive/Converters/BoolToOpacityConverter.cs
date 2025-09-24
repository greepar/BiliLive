using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace BiliLive.Converters;

public class BoolToOpacityConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? 0.0 : 1.0; // true → 不透明, false → 透明
        return 0.0;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double d)
            return d >= 0.5; // 大于 0.5 认为是 true
        return false;
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}