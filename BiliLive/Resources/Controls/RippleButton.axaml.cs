using System;
using System.Windows.Input;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace BiliLive.Resources.Controls
{
    public partial class RippleButton : UserControl
    {
        // public static readonly StyledProperty<ICommand?> CommandProperty =
        //     AvaloniaProperty.Register<RippleButton, ICommand?>(nameof(Command));
        //
        // public static readonly StyledProperty<object?> ContentProperty =
        //     AvaloniaProperty.Register<RippleButton, object?>(nameof(Content));
        //
        // public static readonly StyledProperty<object?> CommandParameterProperty =
        //     AvaloniaProperty.Register<RippleButton, object?>(nameof(CommandParameter));
        //
        // public ICommand? Command
        // {
        //     get => GetValue(CommandProperty);
        //     set => SetValue(CommandProperty, value);
        // }
        //
        // public object? Content
        // {
        //     get => GetValue(ContentProperty);
        //     set => SetValue(ContentProperty, value);
        // }
        //
        // public object? CommandParameter
        // {
        //     get => GetValue(CommandParameterProperty);
        //     set => SetValue(CommandParameterProperty, value);
        // }
        //
        // public RippleButton()
        // {
        //     InitializeComponent();
        // }
        //
        // private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        // {
        //     var point = e.GetPosition(PART_Canvas);
        //
        //     var ripple = new Ellipse
        //     {
        //         Width = 0,
        //         Height = 0,
        //         Fill = new SolidColorBrush(Color.FromArgb(60, 255, 255, 255)),
        //         RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative),
        //         RenderTransform = new ScaleTransform(1, 1),
        //         [Canvas.LeftProperty] = point.X,
        //         [Canvas.TopProperty] = point.Y
        //     };
        //
        //     PART_Canvas.Children.Add(ripple);
        //
        //     var duration = TimeSpan.FromMilliseconds(500);
        //
        //     var anim = new Animation
        //     {
        //         Duration = duration,
        //         FillMode = FillMode.Forward,
        //         Children =
        //         {
        //             new KeyFrame
        //             {
        //                 Cue = new Cue(0),
        //                 Setters =
        //                 {
        //                     new Setter(ScaleTransform.ScaleXProperty, 0.1),
        //                     new Setter(ScaleTransform.ScaleYProperty, 0.1),
        //                     new Setter(Ellipse.OpacityProperty, 0.4),
        //                 }
        //             },
        //             new KeyFrame
        //             {
        //                 Cue = new Cue(1),
        //                 Setters =
        //                 {
        //                     new Setter(ScaleTransform.ScaleXProperty, 6.0),
        //                     new Setter(ScaleTransform.ScaleYProperty, 6.0),
        //                     new Setter(Ellipse.OpacityProperty, 0.0),
        //                 }
        //             }
        //         }
        //     };
        //
        //     _ = anim.RunAsync(ripple).ContinueWith(_ =>
        //     {
        //         Dispatcher.UIThread.InvokeAsync(() =>
        //         {
        //             PART_Canvas.Children.Remove(ripple);
        //         });
        //     });
        //
        //     // 同时手动触发命令（保证点击生效）
        //     if (Command?.CanExecute(CommandParameter) == true)
        //     {
        //         Command.Execute(CommandParameter);
        //     }
        // }
    }
}
