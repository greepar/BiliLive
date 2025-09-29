using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.Threading;

namespace BiliLive.Views.MainWindow;

public partial class MainWindow : Window
{
    // 账号窗口
    private CancellationTokenSource? _animationCts;
    private double _initialNavBarWidth;

    private bool _isTargetVisible = true;

    public MainWindow()
    {
        InitializeComponent();

        SideNavBar.AttachedToVisualTree += async (_, _) =>
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

    
    
    private async void AccountBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            // 取消任何正在进行的动画
            _animationCts?.Cancel();
            // 为本次动画创建一个新取消令牌
            _animationCts = new CancellationTokenSource();
       

            // 在动画进行时禁用交互
            LoginBorder.IsHitTestVisible = false;

             if (_isTargetVisible)
            {
                LoginBorder.IsVisible = true;
                _isTargetVisible = false;
                // var animation = new Animation
                // {
                //     Duration = TimeSpan.FromMilliseconds(300),
                //     // Easing = new ExponentialEaseOut(),
                //     Easing = new BackEaseOut(),
                //     
                //     FillMode = FillMode.Forward,
                //     Children =
                //     {
                //         new KeyFrame
                //         {
                //             Cue = new Cue(1d),
                //             Setters =
                //             {
                //                 new Setter(OpacityProperty, 1.0),
                //                 new Setter(TranslateTransform.XProperty, 0.0)
                //             }
                //         }
                //     }
                // };
                // await animation.RunAsync(LoginBorder, _animationCts.Token);
                
                var opacityAnim = new Animation
                {
                    Duration = TimeSpan.FromMilliseconds(300),
                    Easing = new ExponentialEaseOut(),
                    FillMode = FillMode.Forward,
                    Children =
                    {
                        new KeyFrame
                        {
                            Cue = new Cue(1d),
                            Setters =
                            {
                                new Setter(OpacityProperty, 1.0)
                            }
                        }
                    }
                };

                var translateAnim = new Animation
                {
                    Duration = TimeSpan.FromMilliseconds(300),
                    Easing = new BackEaseOut(),
                    FillMode = FillMode.Forward,
                    Children =
                    {
                        new KeyFrame
                        {
                            Cue = new Cue(1d),
                            Setters =
                            {
                                new Setter(TranslateTransform.XProperty, 0.0)
                            }
                        }
                    }
                };
                
                // 并行播放
                await Task.WhenAll(
                    opacityAnim.RunAsync(LoginBorder, _animationCts.Token),
                    translateAnim.RunAsync(LoginBorder, _animationCts.Token)
                );

                
            }
            else
            {
                _isTargetVisible = true;

                var backAnimation = new Animation
                {
                    Duration = TimeSpan.FromMilliseconds(300),
                    Easing = new ExponentialEaseOut(),
                    FillMode = FillMode.Forward,
                    Children =
                    {
                        new KeyFrame
                        {
                            Cue = new Cue(1d),
                            Setters =
                            {
                                new Setter(OpacityProperty, 0.0),
                                new Setter(TranslateTransform.XProperty, -50.0)
                            }
                        }
                    }
                };
                await backAnimation.RunAsync(LoginBorder, _animationCts.Token);

                // 只有在动画正常完成后才隐藏
                if (!_animationCts.Token.IsCancellationRequested) LoginBorder.IsVisible = false;
            }
        }
        catch (OperationCanceledException)
        {
            // 这是预期的行为，当一个新动画开始时，旧动画被取消。
        }
        catch (Exception)
        {
            // 处理其他可能的异常
        }
        finally
        {
            LoginBorder.IsHitTestVisible = true;
        }
    }


    // 菜单栏伸缩
    private void MenuOpenBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        if (SideNavBar.Width < 100)
            SideNavBar.Width = _initialNavBarWidth;
        else
            SideNavBar.Width = 60;
    }

    private async void SwitchPageAnime(object? sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton btn)
        {
            btn.IsChecked = true;
        }
        
        ContentBorder.Opacity = 0;
        ContentBorder.RenderTransform = new ScaleTransform(0.9, 0.9);
        
        var animation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(300),
            Easing = new QuarticEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(ScaleTransform.ScaleXProperty, 1.0d),
                        new Setter(ScaleTransform.ScaleYProperty, 1.0d),
                        new Setter(OpacityProperty, 1.0),
                    }
                }
            }
        };
        
        await animation.RunAsync(ContentBorder);
    }
}