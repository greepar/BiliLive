using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;

namespace BiliLive.Resources.Controls;

public class SvgBox : Viewbox
{
    public static readonly StyledProperty<Geometry?> PathProperty =
        AvaloniaProperty.Register<SvgBox, Geometry?>(nameof(PathData));

    public Geometry? PathData
    {
        get => GetValue(PathProperty);
        set => SetValue(PathProperty, value);
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
        Child = CreatePath();
    }

    private Control CreatePath()
    {
        var innerPath = new Path
        {
            Opacity = 1,
            RenderTransform = new TranslateTransform(0, 960)
        };

        
        innerPath.Bind(
            Shape.FillProperty,
            this.GetObservable(FillProperty)
        );
        
        innerPath.Bind(
            Avalonia.Controls.Shapes.Path.DataProperty,
            this.GetObservable(PathProperty)
        );

        return innerPath;
    }
}