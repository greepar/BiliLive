using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Material3.UI.Controls.CustomControl;

public enum RippleStackingMode
{
    All,
    LatestOnly
}

public class InkRipple : Control
{
    public static readonly StyledProperty<IBrush?> BrushProperty =
        AvaloniaProperty.Register<InkRipple, IBrush?>(nameof(Brush));
    
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<InkRipple, CornerRadius>(nameof(CornerRadius));

    public static readonly StyledProperty<bool> BoundedProperty =
        AvaloniaProperty.Register<InkRipple, bool>(nameof(Bounded), true);

    public static readonly StyledProperty<double> BaseOpacityProperty =
        AvaloniaProperty.Register<InkRipple, double>(nameof(BaseOpacity), 0.1);
    
    public static readonly StyledProperty<TimeSpan> GrowDurationProperty =
        AvaloniaProperty.Register<InkRipple, TimeSpan>(nameof(GrowDuration), TimeSpan.FromMilliseconds(300));
    
    public static readonly StyledProperty<TimeSpan> FadeInDurationProperty =
        AvaloniaProperty.Register<InkRipple, TimeSpan>(nameof(FadeInDuration), TimeSpan.FromMilliseconds(100));
    
    public static readonly StyledProperty<TimeSpan> FadeOutDurationProperty =
        AvaloniaProperty.Register<InkRipple, TimeSpan>(nameof(FadeOutDuration), TimeSpan.FromMilliseconds(200));
    
    public static readonly StyledProperty<RippleStackingMode> StackingModeProperty =
        AvaloniaProperty.Register<InkRipple, RippleStackingMode>(nameof(StackingMode));

    public static readonly StyledProperty<Easing?> GrowEasingProperty =
        AvaloniaProperty.Register<InkRipple, Easing?>(nameof(GrowEasing), new SplineEasing());

    
    public IBrush? Brush
    {
        get => GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    
    public bool Bounded
    {
        get => GetValue(BoundedProperty);
        set => SetValue(BoundedProperty, value);
    }
    
    public double BaseOpacity
    {
        get => GetValue(BaseOpacityProperty);
        set => SetValue(BaseOpacityProperty, value);
    }

    public TimeSpan GrowDuration
    {
        get => GetValue(GrowDurationProperty);
        set => SetValue(GrowDurationProperty, value);
    }
    
    public TimeSpan FadeInDuration
    {
        get => GetValue(FadeInDurationProperty);
        set => SetValue(FadeInDurationProperty, value);
    }
    
    public TimeSpan FadeOutDuration
    {
        get => GetValue(FadeOutDurationProperty);
        set => SetValue(FadeOutDurationProperty, value);
    }
    
    public RippleStackingMode StackingMode
    {
        get => GetValue(StackingModeProperty);
        set => SetValue(StackingModeProperty, value);
    }

    public Easing? GrowEasing
    {
        get => GetValue(GrowEasingProperty);
        set => SetValue(GrowEasingProperty, value);
    }
    
    static InkRipple()
    {
        AffectsRender<InkRipple>(BrushProperty, CornerRadiusProperty);
    }

    public InkRipple()
    {
        IsHitTestVisible = false;
    }
    
    private readonly List<RippleParticle> _ripples = new();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TemplatedParent is InputElement host)
        {
            host.AddHandler(PointerPressedEvent, OnPressed, RoutingStrategies.Tunnel);
            host.AddHandler(PointerReleasedEvent, OnReleased, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            host.AddHandler(PointerCaptureLostEvent, OnReleased, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
            host.AddHandler(PointerExitedEvent, OnPointerExited, RoutingStrategies.Tunnel);
        }
    }
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (TemplatedParent is InputElement host)
        {
            host.RemoveHandler(PointerPressedEvent, OnPressed);
            host.RemoveHandler(PointerReleasedEvent, OnReleased);
            host.RemoveHandler(PointerCaptureLostEvent, OnReleased);
            host.RemoveHandler(PointerExitedEvent, OnPointerExited);
        }
        _ripples.Clear();
        base.OnDetachedFromVisualTree(e);
    }
    
    private void OnParticlePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == RippleParticle.RadiusProperty || e.Property == RippleParticle.OpacityProperty)
            InvalidateVisual();
    }

    private void AttachRipple(RippleParticle rp)
    {
        rp.PropertyChanged += OnParticlePropertyChanged;
    }

    private void DetachRipple(RippleParticle rp)
    {
        rp.PropertyChanged -= OnParticlePropertyChanged;
    }
    
    private void RemoveRipple(RippleParticle rp)
    {
        DetachRipple(rp);
        _ripples.Remove(rp);
    }

    private void ClearRipples(bool immediate)
    {
        if (immediate)
        {
            foreach (var rp in _ripples.ToArray())
                RemoveRipple(rp);
            InvalidateVisual();
        }
        else
        {
            foreach (var rp in _ripples.ToArray())
                _ = BeginFadeOutAndCleanup(rp);
        }
    }
    
    private void OnPressed(object? s, PointerPressedEventArgs e)
    {
        if (Brush is null) return;

        if (StackingMode == RippleStackingMode.LatestOnly)
            ClearRipples(immediate: true);
        
        var position = e.GetPosition(this);
        var maxR = ComputeMaxRadius(position);

        var rp = new RippleParticle { Center = position, MaxRadius = maxR, Opacity = 0, Radius = 0 };
        rp.ConfigureGrow(GrowDuration, FadeInDuration, GrowEasing);

        AttachRipple(rp);
        _ripples.Add(rp);

        rp.Radius = maxR;
        rp.Opacity = BaseOpacity;

        InvalidateVisual();
    }

    private void OnReleased(object? s, PointerEventArgs e)
    {
        foreach (var rp in _ripples.ToList())
            _ = BeginFadeOutAndCleanup(rp);
    }

    private void OnPointerExited(object? s, PointerEventArgs e)
    {
        OnReleased(s, e);
    }

    private async Task BeginFadeOutAndCleanup(RippleParticle rp)
    {
        if (!_ripples.Contains(rp)) return;
        
        rp.ConfigureFadeOut(FadeOutDuration);
        rp.Opacity = 0;

        try
        {
            await Task.Delay(FadeOutDuration);
        }
        catch { /* ignore */ }

        if (_ripples.Contains(rp))
        {
            DetachRipple(rp);
            _ripples.Remove(rp);
        }

        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        if (Brush is null || _ripples.Count == 0) return;

        var rect = new Rect(Bounds.Size);

        if (Bounded)
        {
            using (context.PushClip(new RoundedRect(rect, CornerRadius)))
                DrawAll(context);
        }
        else
        {
            DrawAll(context);
        }
    }

    private void DrawAll(DrawingContext context)
    {
        foreach (var rp in _ripples)
        {
            var r = rp.Radius;
            var a = rp.Opacity;
            if (r <= 0 || a <= 0) continue;

            using (context.PushOpacity(a))
                context.DrawEllipse(Brush, pen: null, rp.Center, r, r);
        }
    }

    private double ComputeMaxRadius(Point p)
    {
        var corners = new[]
        {
            new Point(0,0),
            new Point(Bounds.Width, 0),
            new Point(0, Bounds.Height),
            new Point(Bounds.Width, Bounds.Height)
        };
        return corners.Select(c => Math.Sqrt((c.X - p.X)*(c.X - p.X) + (c.Y - p.Y)*(c.Y - p.Y))).Max();
    }
}