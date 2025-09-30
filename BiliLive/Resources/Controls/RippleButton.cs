using Avalonia.Animation.Easings;
using Avalonia.Input;

namespace BiliLive.Resources.Controls;

using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;
using System;

public class RippleButton : Button
{
    public static readonly StyledProperty<IBrush> RippleColorProperty =
        AvaloniaProperty.Register<RippleButton, IBrush>(nameof(RippleColor), 
            Brushes.White);

    public static readonly StyledProperty<double> RippleDurationProperty =
        AvaloniaProperty.Register<RippleButton, double>(nameof(RippleDuration), 0.8);
    
    
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

    private Canvas? _rippleCanvas;
    // private Border _rippleElement;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // 获取模板中的涟漪元素
        // _rippleCanvas = e.NameScope.Find<Canvas>("PART_RippleCanvas");
        // _rippleElement = e.NameScope.Find<Border>("PART_RippleElement");
        
        // 创建一个 border 作为容器

        var radius = this.CornerRadius;
        
        var border = new Border
        {
            ClipToBounds = true,
            Background = Brushes.Transparent,
            CornerRadius = radius
        };
        
        var panel = new Grid()
        {
            
        };
        border.Child = panel;
        
        _rippleCanvas = new Canvas
        {
            ClipToBounds = true
        };
            
      
        
        // 把原来的 Content 放进去
        if (Content is Control contentControl)
        {
            panel.Children.Add(contentControl);
        }
     

        panel.Children.Add(_rippleCanvas);
        Content = border;
        
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        var position = e.GetPosition(this);
        StartRippleAnimation(position);
    }

    private async void StartRippleAnimation(Point position)
    {
        // if (_rippleCanvas == null || _rippleElement == null) return;
        
        var rect = new Rect(Bounds.Size);

        if (Content is Border border)
        {
            border.Width = Bounds.Width;
            border.Height = Bounds.Height;
            Clip = new RectangleGeometry(rect);
            CornerRadius = CornerRadius;
            ClipToBounds = true;
        }
        
        Console.WriteLine(rect.Size);
        // 涟漪元素
        var ripple = new Border
        {
            Width = 100,
            Height = 100,
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
            Duration = TimeSpan.FromSeconds(2.8),
            Easing = new QuinticEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters =
                    {
                        new Setter(OpacityProperty, 1.0),
                        new Setter(ScaleTransform.ScaleXProperty, 1.0),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0)
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
}
