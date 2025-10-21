using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Avalonia.Data.Converters;

namespace BiliLive.Views.MainWindow.Pages.AutoService.Components.Utility
{
    public class MathConverter : IValueConverter
    {
        private static readonly DataTable Computer = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not (IConvertible and not string) || parameter is not string expression)
            {
                return value;
            }

            try
            {
                var formattedExpression = expression.Replace("x", ((IConvertible)value).ToString(culture));
                
                var result = Computer?.Compute(formattedExpression, string.Empty);
                return result;
            }
            catch (Exception)
            {
                return value;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            //无此方法
            return null;
        }
    }
}