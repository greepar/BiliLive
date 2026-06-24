using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Material3.UI.Markup;

/// <summary>
/// XAML markup extension that parses an SVG path string into a <see cref="StreamGeometry"/>.
/// Use as: <c>{m3:Geometry {x:Static m3:Icons.Home}}</c>.
/// </summary>
public class GeometryExtension(string data) : MarkupExtension
{
    private string Data { get; } = data;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return StreamGeometry.Parse(Data);
    }
}
