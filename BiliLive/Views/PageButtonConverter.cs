using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using BiliLive.Views.MainWindow;

namespace BiliLive.Views;

public class PageButtonConverter: MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NavigationPage selected && parameter is NavigationPage current)
        {
            return selected == current;
        }
        return false;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new Exception();
    
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}