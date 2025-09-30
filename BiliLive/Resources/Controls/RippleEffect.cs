using Avalonia.Animation.Easings;
using Avalonia.Input;

namespace BiliLive.Resources.Controls;

using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using System;

public class RippleBorder : Border
{
    public static readonly StyledProperty<IBrush> RippleColorProperty =
        AvaloniaProperty.Register<RippleBorder, IBrush>(nameof(RippleColor), 
            Brushes.White);

    public static readonly StyledProperty<double> RippleDurationProperty =
        AvaloniaProperty.Register<RippleBorder, double>(nameof(RippleDuration), 0.8);
    
 
    
    public IBrush RippleColor
    {
        get => GetValue(RippleColorProperty);
        set => SetValue(RippleColorProperty, value);
    }

    public double RippleDuration
    {
        get => GetValue(RippleDurationProperty);
        set => SetValue(RippleDurationProperty, value);
    }
    
    private readonly Canvas _rippleCanvas;
    
    public RippleBorder()
    {
        _rippleCanvas = new Canvas();
        Child = _rippleCanvas;
        ClipToBounds = true;
    }
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        var position = e.GetPosition(this);
        StartRippleAnimation(position);
    }

    private async void StartRippleAnimation(Point position)
    {
        try
        {
            
            // 计算从点击位置到最远边界的距离
            var toLeft = position.X;
            var toRight = Bounds.Width - position.X;
            var toTop = position.Y;
            var toBottom = Bounds.Height - position.Y;
        
            // 取水平方向的最大距离，确保覆盖左右 边界
            var maxHorizontal = Math.Max(toLeft, toRight);
        
            // 如果需要同时覆盖上下边界，可以取对角线距离
            var maxVertical = Math.Max(toTop, toBottom);
            var maxDistance = Math.Sqrt(maxHorizontal * maxHorizontal + maxVertical * maxVertical);
            
            // 涟漪元素
            var ripple = new Border
            {
                Width = maxDistance,
                Height = maxDistance,
                Background = RippleColor,
                CornerRadius = new CornerRadius(100),
                RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
                RenderTransform = new ScaleTransform(0, 0)
            };

            Canvas.SetLeft(ripple, position.X - ripple.Width / 2);
            Canvas.SetTop(ripple, position.Y - ripple.Height / 2);
        
            _rippleCanvas.Children.Add(ripple);

            var animation = new Animation
            {
                Duration = TimeSpan.FromSeconds(RippleDuration),
                Easing = new SineEaseOut(),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0.0),
                        Setters =
                        {
                            new Setter(OpacityProperty, 1.0),
                            new Setter(ScaleTransform.ScaleXProperty, 0.5),
                            new Setter(ScaleTransform.ScaleYProperty, 0.5)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1.0),
                        Setters =
                        {
                            new Setter(OpacityProperty, 0.0),
                            new Setter(ScaleTransform.ScaleXProperty, 3.0),
                            new Setter(ScaleTransform.ScaleYProperty, 3.0)
                        }
                    }
                }
            };

            await animation.RunAsync(ripple);

            // 动画结束移除
            _rippleCanvas.Children.Remove(ripple);
        }
        catch (Exception e)
        {
            // ignored
        }
      
    }
}
