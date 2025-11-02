using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace BiliLive.Resources.Controls;

public class SvgBox : Viewbox
{
    public static readonly StyledProperty<Geometry?> PathDataProperty =
        AvaloniaProperty.Register<SvgBox, Geometry?>(nameof(PathData));

    public Geometry? PathData
    {
        get => GetValue(PathDataProperty);
        set => SetValue(PathDataProperty, value);
    }

    public static readonly StyledProperty<IBrush?> FillProperty =
        AvaloniaProperty.Register<SvgBox, IBrush?>(nameof(Fill), defaultValue: Brushes.Black);

    public IBrush? Fill
    {
        get => GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public SvgBox()
    {
        var innerPath = new Path
        {
            Opacity = 1,
            RenderTransform = new TranslateTransform(0, 960)
        };

        innerPath.Bind(Shape.FillProperty, this.GetObservable(FillProperty));
        innerPath.Bind(Path.DataProperty, this.GetObservable(PathDataProperty));

        Child = innerPath;
    }
}