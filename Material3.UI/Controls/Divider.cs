using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Material3.UI.Controls;

/// <summary>
/// Material 3 horizontal or vertical divider. A 1px line drawn in the
/// theme's OutlineVariant brush by default.
/// </summary>
public class Divider : Control
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<Divider, Orientation>(nameof(Orientation), Orientation.Horizontal);

    public static readonly StyledProperty<IBrush?> BrushProperty =
        AvaloniaProperty.Register<Divider, IBrush?>(nameof(Brush));

    public static readonly StyledProperty<double> ThicknessProperty =
        AvaloniaProperty.Register<Divider, double>(nameof(Thickness), 1.0);

    public static readonly StyledProperty<double> InsetStartProperty =
        AvaloniaProperty.Register<Divider, double>(nameof(InsetStart));

    public static readonly StyledProperty<double> InsetEndProperty =
        AvaloniaProperty.Register<Divider, double>(nameof(InsetEnd));

    public Orientation Orientation { get => GetValue(OrientationProperty); set => SetValue(OrientationProperty, value); }
    public IBrush? Brush { get => GetValue(BrushProperty); set => SetValue(BrushProperty, value); }
    public double Thickness { get => GetValue(ThicknessProperty); set => SetValue(ThicknessProperty, value); }
    public double InsetStart { get => GetValue(InsetStartProperty); set => SetValue(InsetStartProperty, value); }
    public double InsetEnd { get => GetValue(InsetEndProperty); set => SetValue(InsetEndProperty, value); }

    static Divider()
    {
        AffectsRender<Divider>(BrushProperty, ThicknessProperty, OrientationProperty, InsetStartProperty, InsetEndProperty);
        AffectsMeasure<Divider>(ThicknessProperty, OrientationProperty);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        return Orientation == Orientation.Horizontal
            ? new Size(0, Thickness)
            : new Size(Thickness, 0);
    }

    public override void Render(DrawingContext context)
    {
        var brush = Brush ?? Brushes.Transparent;
        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;

        if (Orientation == Orientation.Horizontal)
        {
            var y = h / 2;
            var rect = new Rect(InsetStart, y - Thickness / 2, Math.Max(0, w - InsetStart - InsetEnd), Thickness);
            context.FillRectangle(brush, rect);
        }
        else
        {
            var x = w / 2;
            var rect = new Rect(x - Thickness / 2, InsetStart, Thickness, Math.Max(0, h - InsetStart - InsetEnd));
            context.FillRectangle(brush, rect);
        }
    }
}
