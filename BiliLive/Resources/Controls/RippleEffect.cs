// using Avalonia.Animation.Easings;
//
// namespace BiliLive.Resources.Controls;
//
// using Avalonia;
// using Avalonia.Animation;
// using Avalonia.Controls;
// using Avalonia.Media;
// using Avalonia.Styling;
// using System;
//
// public class RippleEffect : Control
// {
//     private double _rippleSize;
//     private Point _rippleCenter;
//     private bool _isRippling;
//
//     public static readonly StyledProperty<IBrush> RippleColorProperty =
//         AvaloniaProperty.Register<RippleEffect, IBrush>(nameof(RippleColor), Brushes.White);
//
//     public IBrush RippleColor
//     {
//         get => GetValue(RippleColorProperty);
//         set => SetValue(RippleColorProperty, value);
//     }
//
//     public void StartRipple()
//     {
//         if (_isRippling) return;
//
//         _isRippling = true;
//         _rippleCenter = new Point(Bounds.Width / 2, Bounds.Height / 2);
//         _rippleSize = Math.Max(Bounds.Width, Bounds.Height) * 2;
//
//         var animation = new Animation
//         {
//             Duration = TimeSpan.FromMilliseconds(600),
//             Easing = new CubicEaseOut(),
//             Children =
//             {
//                 new KeyFrame
//                 {
//                     Cue = new Cue(0.0),
//                     Setters =
//                     {
//                         new Setter(OpacityProperty, 0.6),
//                         new Setter(ScaleTransform.ScaleXProperty, 0.0),
//                         new Setter(ScaleTransform.ScaleYProperty, 0.0)
//                     }
//                 },
//                 new KeyFrame
//                 {
//                     Cue = new Cue(1.0),
//                     Setters =
//                     {
//                         new Setter(OpacityProperty, 0.0),
//                         new Setter(ScaleTransform.ScaleXProperty, 1.0),
//                         new Setter(ScaleTransform.ScaleYProperty, 1.0)
//                     }
//                 }
//             }
//         };
//
//         animation.RunAsync(this).ContinueWith(_ => _isRippling = false);
//         InvalidateVisual();
//     }
//
//     public override void Render(DrawingContext context)
//     {
//         base.Render(context);
//
//         if (_isRippling)
//         {
//             var ellipseBrush = RippleColor;
//             if (ellipseBrush != null)
//             {
//                 var ellipseGeometry = new EllipseGeometry(
//                     new Rect(_rippleCenter.X - _rippleSize / 2, 
//                             _rippleCenter.Y - _rippleSize / 2, 
//                             _rippleSize, _rippleSize));
//                 
//                 context.DrawGeometry(ellipseBrush, null, ellipseGeometry);
//             }
//         }
//     }
// }
