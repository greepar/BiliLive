using System;
using Avalonia;
using Avalonia.Controls;

namespace BiliLive.Views.MainWindow.Pages.AutoService.Components.Utility;

public class CircularPanel : Panel
{
    public static readonly StyledProperty<double> RadiusFactorProperty =
        AvaloniaProperty.Register<CircularPanel, double>(nameof(RadiusFactor), 1.0);

    public double RadiusFactor
    {
        get => GetValue(RadiusFactorProperty);
        set => SetValue(RadiusFactorProperty, value);
    }
    
    public static readonly StyledProperty<double> AngleOffsetProperty =
        AvaloniaProperty.Register<CircularPanel, double>(nameof(AngleOffset), -90.0);

    public double AngleOffset
    {
        get => GetValue(AngleOffsetProperty);
        set => SetValue(AngleOffsetProperty, value);
    }
    
    public static readonly StyledProperty<int> IndexPositionOffsetProperty =
        AvaloniaProperty.Register<CircularPanel, int>(nameof(IndexPositionOffset), 0);

    public int IndexPositionOffset
    {
        get => GetValue(IndexPositionOffsetProperty);
        set => SetValue(IndexPositionOffsetProperty, value);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (Children.Count == 0)
            return finalSize;

        double angleStep = 360.0 / Children.Count;
        Point center = new Point(finalSize.Width / 2, finalSize.Height / 2);
        double radius = Math.Min(finalSize.Width, finalSize.Height) / 2.0 * RadiusFactor;

        for (int i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            
            double logicalPosition = (i + IndexPositionOffset) % Children.Count;
            if (logicalPosition < 0) logicalPosition += Children.Count;
            
            var angle = (logicalPosition * angleStep + AngleOffset) * (Math.PI / 180.0);

            var x = center.X + Math.Cos(angle) * radius - child.DesiredSize.Width / 2;
            var y = center.Y + Math.Sin(angle) * radius - child.DesiredSize.Height / 2;

            child.Arrange(new Rect(new Point(x, y), child.DesiredSize));
        }

        return finalSize;
    }
}