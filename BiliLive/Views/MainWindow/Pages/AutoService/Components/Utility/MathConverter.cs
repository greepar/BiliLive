using System;
using System.Globalization;
using System.Text;
using Avalonia.Data.Converters;

namespace BiliLive.Views.MainWindow.Pages.AutoService.Components.Utility;

public class MathConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not (IConvertible and not string) || parameter is not string expression)
            return value;

        try
        {
            var formattedExpression = expression.Replace("x", ((IConvertible)value).ToString(culture));
            var result = MiniMathParser.Eval(formattedExpression);
            return result;
        }
        catch
        {
            return value;
        }
    }
    
    private static class MiniMathParser
    {
        public static double Eval(string expr)
        {
            var s = new StringBuilder(expr.Replace(" ", ""));
            return ParseExpr(s);
        }

        private static double ParseExpr(StringBuilder s)
        {
            var x = ParseTerm(s);
            while (s.Length > 0 && (s[0] == '+' || s[0] == '-'))
            {
                var op = s[0];
                s.Remove(0, 1);
                var y = ParseTerm(s);
                x = op == '+' ? x + y : x - y;
            }
            return x;
        }

        private static double ParseTerm(StringBuilder s)
        {
            var x = ParseFactor(s);
            while (s.Length > 0 && (s[0] == '*' || s[0] == '/'))
            {
                var op = s[0];
                s.Remove(0, 1);
                var y = ParseFactor(s);
                x = op == '*' ? x * y : x / y;
            }
            return x;
        }

        private static double ParseFactor(StringBuilder s)
        {
            if (s[0] == '(')
            {
                s.Remove(0, 1);
                var x = ParseExpr(s);
                if (s.Length == 0 || s[0] != ')')
                    throw new FormatException("Missing closing parenthesis");
                s.Remove(0, 1);
                return x;
            }

            var i = 0;
            // 允许负号、小数点、科学计数法
            while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.' || s[i] == 'e' || s[i] == 'E' || (i == 0 && s[i] == '-')))
                i++;

            var numStr = s.ToString(0, i);
            if (!double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
                throw new FormatException($"Invalid number: {numStr}");

            s.Remove(0, i);
            return num;
        }
    }
    
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // 无此方法
        return null;
    }
}