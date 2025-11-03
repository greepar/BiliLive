using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace BiliLive.Resources.Controls;

public class ProgressRing : Control
{
    private readonly DispatcherTimer _timer;
    private double _angle;

    public static readonly StyledProperty<double> StrokeThicknessProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(StrokeThickness), 6);

    public static readonly StyledProperty<IBrush> StrokeProperty =
        AvaloniaProperty.Register<ProgressRing, IBrush>(nameof(Stroke), Brushes.MediumPurple);

    public static readonly StyledProperty<double> SpeedProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(Speed), 180d); // 度/秒

    public static readonly StyledProperty<double> SweepAngleProperty =
        AvaloniaProperty.Register<ProgressRing, double>(nameof(SweepAngle), 270d); // 弧长（度）
    
    public static readonly StyledProperty<bool> IsRotatingProperty =
        AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsRotating));

    public double StrokeThickness
    {
        get => GetValue(StrokeThicknessProperty);
        set => SetValue(StrokeThicknessProperty, value);
    }

    public IBrush Stroke
    {
        get => GetValue(StrokeProperty);
        set => SetValue(StrokeProperty, value);
    }

    public double Speed
    {
        get => GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public double SweepAngle
    {
        get => GetValue(SweepAngleProperty);
        set => SetValue(SweepAngleProperty, value);
    }

    public bool IsRotating
    {
        get => GetValue(IsRotatingProperty);
        set => SetValue(IsRotatingProperty, value);
    }
    
    private double _elapsed;
    private const double AnimationDuration = 2000.0; // 动画周期
    public ProgressRing()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) }; // 约 60fps
        _timer.Tick += (_, _) =>
        {
            if (!IsRotating)
            {
                _timer.IsEnabled = false;
                return;
            };
            _elapsed = (_elapsed + _timer.Interval.TotalMilliseconds) % AnimationDuration;
            var progress = _elapsed / AnimationDuration;
            SweepAngle = GetMaterialSweep(progress);
            
            _angle = (360 * progress * 2 + Speed * _timer.Interval.TotalSeconds) % 360;
            // _angle = (360 * progress + Speed * _timer.Interval.TotalSeconds) % 360;

            // _angle = (_angle + Speed * _timer.Interval.TotalSeconds) % 360;
            InvalidateVisual();
        };
        _timer.Start();
    }

    private static double EaseInOut(double t)
    {
        return Math.Cos((t + 1) * Math.PI) / 2.0 + 0.5;
    }

    private static double GetMaterialSweep(double time)
    {
        // time ∈ [0,1)
        const double maxSweep = 260; // 最大弧长
        double sweep;

        if (time < 0.5)
        {
            // 前半段：弧长变长
            sweep = EaseInOut(time * 2) * maxSweep;
        }
        else
        {
            // 后半段：弧长变短
            sweep = (1 - EaseInOut((time - 0.5) * 2)) * maxSweep;
        }

        return sweep;
    }

    
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var w = Bounds.Width;
        var h = Bounds.Height;
        if (w <= 0 || h <= 0)
            return;

        var size = Math.Min(w, h);
        var radius = (size - StrokeThickness) / 2.0;
        var center = new Point(w / 2.0, h / 2.0);

        // 弧线的起止角度
        var startAngleDeg = _angle;
        var endAngleDeg = _angle + SweepAngle;

        // 转为弧度
        var startRad = startAngleDeg * Math.PI / 180.0;
        var endRad = endAngleDeg * Math.PI / 180.0;

        //固定空隙弧度
        var gapRad = ((SweepAngle > 280.0) || !IsRotating) ? 0 : 35.0 * Math.PI / 180.0 ;
        
        // 计算起点/终点
        var startPoint = new Point(
            center.X + radius * Math.Cos(startRad),
            center.Y + radius * Math.Sin(startRad));

        var endPoint = new Point(
            center.X + radius * Math.Cos(endRad),
            center.Y + radius * Math.Sin(endRad));
        
        var isLargeArc = SweepAngle > 180.0;
        
        //计算空隙留出之后的弧度
        var restStartPoint = new Point(
            center.X + radius * Math.Cos(endRad + gapRad),
            center.Y + radius * Math.Sin(endRad + gapRad));
        
        var restEndPoint = new Point(
            center.X + radius * Math.Cos(startRad - gapRad),
            center.Y + radius * Math.Sin(startRad - gapRad));
        
        var restIsLargeArc = gapRad == 0 ? SweepAngle < 180 : SweepAngle < 110;
     

        // 绘制弧线
        var arcGeometry = new StreamGeometry();
        using (var ctx = arcGeometry.Open())
        {
            ctx.BeginFigure(startPoint, false);
            ctx.ArcTo(endPoint, new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise);
        }
        
        var pen = new Pen(Stroke, StrokeThickness);
        context.DrawGeometry(null, pen, arcGeometry);

        //绘制留出空隙的弧线 
        var wheatStroke = Brush.Parse("#E1E0F7") ;
        var restArcGeometry = new StreamGeometry();
        using (var ctx = restArcGeometry.Open())
        {
            ctx.BeginFigure(restStartPoint, false);
            ctx.ArcTo(restEndPoint, new Size(radius, radius), 0, restIsLargeArc, SweepDirection.Clockwise);
        }
        
        var restPen = new Pen(wheatStroke, StrokeThickness);
        context.DrawGeometry(null, restPen, restArcGeometry);
        
        //PenLineCap.Round
        // 绘制两端圆头
        var capRadius = StrokeThickness / 2.0;
        DrawCircle(context, startPoint, capRadius, Stroke);
        DrawCircle(context, endPoint, capRadius, Stroke);
        
        // 绘制留出空隙的圆头
        if (!(SweepAngle <= 270) || !IsRotating ) return;
        DrawCircle(context, restStartPoint, capRadius, wheatStroke);
        DrawCircle(context, restEndPoint, capRadius, wheatStroke);
    }

    private static void DrawCircle(DrawingContext context, Point center, double radius, IBrush brush)
    {
        var circle = new StreamGeometry();
        using (var ctx = circle.Open())
        {
            ctx.BeginFigure(new Point(center.X + radius, center.Y), true);
            ctx.ArcTo(new Point(center.X - radius, center.Y),
                new Size(radius, radius), 0, true, SweepDirection.Clockwise);
            ctx.ArcTo(new Point(center.X + radius, center.Y),
                new Size(radius, radius), 0, true, SweepDirection.Clockwise);
        }

        context.DrawGeometry(brush, null, circle);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }
}

