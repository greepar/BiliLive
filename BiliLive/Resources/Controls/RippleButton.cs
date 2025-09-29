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

    private static readonly StyledProperty<double> RippleDurationProperty =
        AvaloniaProperty.Register<RippleButton, double>(nameof(RippleDuration), 600);

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

    private Canvas _rippleCanvas;
    private Border _rippleElement;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // 获取模板中的涟漪元素
        _rippleCanvas = e.NameScope.Find<Canvas>("PART_RippleCanvas");
        _rippleElement = e.NameScope.Find<Border>("PART_RippleElement");
        // _rippleElement = new Border()
        // {
        //     Width = 50, Height = 50,
        //     CornerRadius = new CornerRadius(100),
        // };
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        var position = e.GetPosition(this);
        StartRippleAnimation(position);
    }

    private async void StartRippleAnimation(Point position)
    {
        if (_rippleCanvas == null || _rippleElement == null) return;

        var rippleHeight = _rippleElement.Height;
        
        _rippleElement.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
        
        // 设置涟漪位置
        Canvas.SetLeft(_rippleElement, position.X - rippleHeight / 2);
        Canvas.SetTop(_rippleElement, position.Y - rippleHeight / 2);

        // 设置涟漪颜色
        _rippleElement.Background = RippleColor;

        // 创建动画
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(RippleDuration),
            Easing = new CubicEaseOut(),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0.0),
                    Setters =
                    {
                        new Setter(OpacityProperty, 1.0),
                        new Setter(ScaleTransform.ScaleXProperty, 0.0),
                        new Setter(ScaleTransform.ScaleYProperty, 0.0)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1.0),
                    Setters =
                    {
                        new Setter(OpacityProperty, 0.0),
                        new Setter(ScaleTransform.ScaleXProperty, 2.5),
                        new Setter(ScaleTransform.ScaleYProperty, 2.5)
                    }
                }
            }
        };

        // 运行动画
        await animation.RunAsync(_rippleElement);
        
        // 动画完成后重置
        _rippleElement.Opacity = 0;
    }
}
