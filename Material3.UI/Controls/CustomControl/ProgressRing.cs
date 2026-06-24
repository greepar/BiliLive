using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Material3.UI.Controls;

/// <summary>
/// Material 3 circular progress indicator.
/// Supports both determinate (a fixed arc representing <see cref="Value"/>)
/// and indeterminate (a rotating, growing/shrinking arc) modes.
/// </summary>
public class ProgressRing : Control
{
    private readonly DispatcherTimer _timer;
    private double _angle;
    private double _elapsed;
    private const double IndeterminateAnimationDuration = 2000.0;

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(StrokeThickness), 6);

    public static readonly StyledProperty<IBrush> StrokeProperty =
        AvaloniaProperty.Register<ProgressRing, IBrush>(nameof(Stroke), Brushes.MediumPurple);

    /// <summary>The brush used to draw the gap (track) arc behind the active arc.</summary>
    public static readonly StyledProperty<IBrush> TrackBrushProperty =
        AvaloniaProperty.Register<ProgressRing, IBrush>(nameof(TrackBrush), new SolidColorBrush(Color.Parse("#E1E0F7")));

    public static readonly StyledProperty<double> SpeedProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(Speed), 180d);

    public static readonly StyledProperty<double> SweepAngleProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(SweepAngle), 270d);

    public static readonly StyledProperty<bool> IsRotatingProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsRotating));

    /// <summary>Determinate value (0..Maximum). Only used when IsIndeterminate is false.</summary>
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(Value));

    public static readonly StyledProperty<double> MinimumProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(Minimum));

    public static readonly StyledProperty<double> MaximumProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(Maximum), 100d);

    /// <summary>
    /// Switch between determinate and indeterminate modes.
    /// Defaults to true so that <c>&lt;ProgressRing IsRotating="True"/&gt;</c>
    /// works without explicit configuration; set false and bind <see cref="Value"/>
    /// for a determinate arc.
    /// </summary>
    public static readonly StyledProperty<bool> IsIndeterminateProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsIndeterminate), true);

    public double StrokeThickness { get => GetValue(StrokeThicknessProperty); set => SetValue(StrokeThicknessProperty, value); }
    public IBrush Stroke { get => GetValue(StrokeProperty); set => SetValue(StrokeProperty, value); }
    public IBrush TrackBrush { get => GetValue(TrackBrushProperty); set => SetValue(TrackBrushProperty, value); }
    public double Speed { get => GetValue(SpeedProperty); set => SetValue(SpeedProperty, value); }
    public double SweepAngle { get => GetValue(SweepAngleProperty); set => SetValue(SweepAngleProperty, value); }
    public bool IsRotating { get => GetValue(IsRotatingProperty); set => SetValue(IsRotatingProperty, value); }
    public double Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public double Minimum { get => GetValue(MinimumProperty); set => SetValue(MinimumProperty, value); }
    public double Maximum { get => GetValue(MaximumProperty); set => SetValue(MaximumProperty, value); }
    public bool IsIndeterminate { get => GetValue(IsIndeterminateProperty); set => SetValue(IsIndeterminateProperty, value); }

    static ProgressRing()
    {
        AffectsRender<ProgressRing>(
            StrokeProperty, StrokeThicknessProperty, TrackBrushProperty,
            ValueProperty, MinimumProperty, MaximumProperty,
            IsIndeterminateProperty, SweepAngleProperty);
    }

    public ProgressRing()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += OnTick;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        UpdateTimerState();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsIndeterminateProperty || change.Property == IsRotatingProperty)
            UpdateTimerState();
    }

    private void UpdateTimerState()
    {
        var shouldRun = IsIndeterminate && IsRotating;
        if (shouldRun && !_timer.IsEnabled) _timer.Start();
        else if (!shouldRun && _timer.IsEnabled) _timer.Stop();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        if (!IsRotating || !IsIndeterminate)
        {
            _timer.Stop();
            return;
        }
        _elapsed = (_elapsed + _timer.Interval.TotalMilliseconds) % IndeterminateAnimationDuration;
        var progress = _elapsed / IndeterminateAnimationDuration;
        SweepAngle = GetMaterialSweep(progress);
        _angle = (360 * progress * 2 + Speed * _timer.Interval.TotalSeconds) % 360;
        InvalidateVisual();
    }

    private static double EaseInOut(double t) => Math.Cos((t + 1) * Math.PI) / 2.0 + 0.5;

    private static double GetMaterialSweep(double time)
    {
        const double maxSweep = 260;
        return time < 0.5
            ? EaseInOut(time * 2) * maxSweep
            : (1 - EaseInOut((time - 0.5) * 2)) * maxSweep;
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0) return;

        var size = Math.Min(w, h);
        var radius = (size - StrokeThickness) / 2.0;
        var center = new Point(w / 2.0, h / 2.0);

        if (!IsIndeterminate)
        {
            DrawDeterminate(context, center, radius);
            return;
        }

        DrawIndeterminate(context, center, radius);
    }

    private void DrawDeterminate(DrawingContext context, Point center, double radius)
    {
        var range = Math.Max(0.0001, Maximum - Minimum);
        var ratio = Math.Clamp((Value - Minimum) / range, 0.0, 1.0);
        var sweep = ratio * 360.0;

        // Background track (full circle)
        var trackPen = new Pen(TrackBrush, StrokeThickness, lineCap: PenLineCap.Round);
        context.DrawEllipse(null, trackPen, center, radius, radius);

        if (ratio <= 0) return;

        // Foreground arc starting at 12-o'clock (-90deg)
        var startDeg = -90.0;
        var endDeg = startDeg + sweep;
        var startRad = startDeg * Math.PI / 180.0;
        var endRad = endDeg * Math.PI / 180.0;

        var startPoint = new Point(center.X + radius * Math.Cos(startRad), center.Y + radius * Math.Sin(startRad));
        var endPoint = new Point(center.X + radius * Math.Cos(endRad), center.Y + radius * Math.Sin(endRad));
        var isLargeArc = sweep > 180.0;

        var arc = new StreamGeometry();
        using (var ctx = arc.Open())
        {
            ctx.BeginFigure(startPoint, false);
            ctx.ArcTo(endPoint, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise);
        }
        var pen = new Pen(Stroke, StrokeThickness, lineCap: PenLineCap.Round);
        context.DrawGeometry(null, pen, arc);
    }

    private void DrawIndeterminate(DrawingContext context, Point center, double radius)
    {
        var startAngleDeg = _angle;
        var endAngleDeg = _angle + SweepAngle;
        var startRad = startAngleDeg * Math.PI / 180.0;
        var endRad = endAngleDeg * Math.PI / 180.0;

        var gapRad = (SweepAngle > 280.0) || !IsRotating ? 0 : 35.0 * Math.PI / 180.0;

        var startPoint = new Point(center.X + radius * Math.Cos(startRad), center.Y + radius * Math.Sin(startRad));
        var endPoint = new Point(center.X + radius * Math.Cos(endRad), center.Y + radius * Math.Sin(endRad));

        var isLargeArc = SweepAngle > 180.0;

        var restStartPoint = new Point(center.X + radius * Math.Cos(endRad + gapRad), center.Y + radius * Math.Sin(endRad + gapRad));
        var restEndPoint = new Point(center.X + radius * Math.Cos(startRad - gapRad), center.Y + radius * Math.Sin(startRad - gapRad));
        var restIsLargeArc = gapRad == 0 ? SweepAngle < 180 : SweepAngle < 110;

        var arc = new StreamGeometry();
        using (var ctx = arc.Open())
        {
            ctx.BeginFigure(startPoint, false);
            ctx.ArcTo(endPoint, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise);
        }
        context.DrawGeometry(null, new Pen(Stroke, StrokeThickness), arc);

        var restArc = new StreamGeometry();
        using (var ctx = restArc.Open())
        {
            ctx.BeginFigure(restStartPoint, false);
            ctx.ArcTo(restEndPoint, new Size(radius, radius), 0, restIsLargeArc, SweepDirection.Clockwise);
        }
        context.DrawGeometry(null, new Pen(TrackBrush, StrokeThickness), restArc);

        var capRadius = StrokeThickness / 2.0;
        DrawCircle(context, startPoint, capRadius, Stroke);
        DrawCircle(context, endPoint, capRadius, Stroke);

        if (!(SweepAngle <= 270) || !IsRotating) return;
        DrawCircle(context, restStartPoint, capRadius, TrackBrush);
        DrawCircle(context, restEndPoint, capRadius, TrackBrush);
    }

    private static void DrawCircle(DrawingContext context, Point center, double radius, IBrush brush)
    {
        var circle = new StreamGeometry();
        using (var ctx = circle.Open())
        {
            ctx.BeginFigure(new Point(center.X + radius, center.Y), true);
            ctx.ArcTo(new Point(center.X - radius, center.Y), new Size(radius, radius), 0, true, SweepDirection.Clockwise);
            ctx.ArcTo(new Point(center.X + radius, center.Y), new Size(radius, radius), 0, true, SweepDirection.Clockwise);
        }
        context.DrawGeometry(brush, null, circle);
    }
}
