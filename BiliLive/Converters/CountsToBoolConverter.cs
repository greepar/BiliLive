using System;
using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using BiliLive.Views.MainWindow.Pages.AutoService;

namespace BiliLive.Converters;

public class CountsToBoolConverter : MarkupExtension ,IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int i)
        {
            return i > 0;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }
    
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}