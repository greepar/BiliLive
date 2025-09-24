using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using BiliLive.Resources;

namespace BiliLive.Converters;

public class BoolToGeometryConverter : MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool b)
            return b ? Geometry.Parse(MdIcons.Check) : Geometry.Parse(MdIcons.Error); 
        return Geometry.Parse(MdIcons.Minimize);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return false;
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}