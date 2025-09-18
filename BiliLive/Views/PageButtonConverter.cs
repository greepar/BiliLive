using System;
using System.Globalization;
using System.Net.Mime;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using BiliLive.Views.MainWindow;

namespace BiliLive.Views;

public class PageButtonConverter: MarkupExtension, IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is NavigationPage selected && parameter is NavigationPage current)
        {
            if (Application.Current != null)
            {
                var btnColor = Application.Current.GetResourceObservable("PrimaryContainer");
                return selected == current ? btnColor : Brushes.Transparent;
            }

        }
        return Brushes.Yellow;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new Exception();
    
    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}