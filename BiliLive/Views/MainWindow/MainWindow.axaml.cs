using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace BiliLive.Views.MainWindow;

public partial class MainWindow : Window
{
    private double _initialNavBarWidth;
    
    public MainWindow()
    {
        InitializeComponent();

        SideNavBar.AttachedToVisualTree += async (_, __) =>
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var w = SideNavBar.Bounds.Width;
                SideNavBar.Width = w;
                _initialNavBarWidth = SideNavBar.Bounds.Width;
            }, DispatcherPriority.Render);
        };
        
        
#if DEBUG
        // Topmost = true;
#endif
    }


    // 拖动窗口
    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
    }

    // 关闭窗口
    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    // 最小化窗口
    private void MinimizeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    

    // 账号窗口
    private async void AccountBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        Console.WriteLine(LoginBorder.IsVisible);
        // var animation = new Animation
        // {
        //     Duration = TimeSpan.FromMilliseconds(300),
        //     Easing = LoginBorder.IsVisible ? new CubicEaseIn() : new CubicEaseOut(),
        //     
        //     Children =
        //     {
        //         new KeyFrame
        //         {
        //             Cue = new Cue(0d),
        //             Setters =
        //             {
        //                 // new Setter(Visual.OpacityProperty, _isPanelVisible ? 1.0 : 0.0),
        //                 new Setter(TranslateTransform.XProperty, LoginBorder.IsVisible ? 0.0 : 100),
        //                 // new Setter(Visual.OpacityProperty, 1),
        //                 // new Setter(TranslateTransform.XProperty, -20),
        //             }
        //         },
        //     }
        // };

      
        
        // var backanimation = new Animation
        // {
        //     Duration = TimeSpan.FromMilliseconds(300),
        //     Easing = new CubicEaseOut(), // 平滑缓动
        //     FillMode = FillMode.Forward, // 保留动画结束时状态
        //     Children =
        //     {
        //         new KeyFrame
        //         {
        //             Cue = new Cue(1d), // 1 表示动画结束
        //             Setters =
        //             {
        //                 new Setter(TranslateTransform.XProperty, 0.0)
        //             }
        //         }
        //     }
        // };
        
        Console.WriteLine(LoginBorder.IsVisible);

        // await animation.RunAsync(LoginBorder);
        // _isPanelVisible = !_isPanelVisible;
    
        // 动画完成后隐藏元素（可选）
        if (!LoginBorder.IsVisible)
        {
            Console.WriteLine("start");
            LoginBorder.IsVisible = true;
            var animation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(300),
                Easing = new ExponentialEaseOut(), // 平滑缓动
                FillMode = FillMode.Forward, // 保留动画结束时状态
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(1d), // 1 表示动画结束
                        Setters =
                        {
                            new Setter(OpacityProperty, 1.0),
                            new Setter(TranslateTransform.XProperty, 0.0)
                        }
                    }
                }
            };
            await animation.RunAsync(LoginBorder);
           Console.WriteLine("done");
        }
        else
        {
            Console.WriteLine("start back");
            var backAnimation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(300),
                Easing = new ExponentialEaseOut(), // 平滑缓动
                FillMode = FillMode.Forward, // 保留动画结束时状态
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(1d), // 1 表示动画结束
                        Setters =
                        {
                            new Setter(OpacityProperty, 0.0),
                            new Setter(TranslateTransform.XProperty, -50.0)
                        }
                    }
                }
            };
            await backAnimation.RunAsync(LoginBorder);
            Console.WriteLine("done back");
            LoginBorder.IsVisible = false;
        }
    }
    


    private void MenuOpenBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        if (SideNavBar.Width < 100)
        {
            SideNavBar.Width = _initialNavBarWidth;
        }
        else
        {
            SideNavBar.Width = 50;
        }
    }
}