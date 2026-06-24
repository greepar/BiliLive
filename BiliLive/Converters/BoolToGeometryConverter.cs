using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Material3.UI.Controls;

namespace BiliLive.Converters;

public class BoolToGeometryConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? Geometry.Parse(Icons.Check) : Geometry.Parse(Icons.Error); 
        return Geometry.Parse(Icons.Minimize);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return false;
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}