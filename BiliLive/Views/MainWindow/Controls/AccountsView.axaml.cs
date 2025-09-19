using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;

namespace BiliLive.Views.MainWindow.Controls;

public partial class AccountsView : UserControl
{
    private CancellationTokenSource? _animationCts;
    private bool _isTargetVisible = true; // 目标元素的初始可见状态

    private readonly Animation _translateAnim = new Animation
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
                    new Setter(TranslateTransform.YProperty, 180.0)
                }
            }
        }
    };
    
    private readonly Animation _backAnimation = new Animation
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
                    new Setter(TranslateTransform.YProperty, 0.0)
                }
            }
        }
    };
    
    
    public AccountsView()
    {
        InitializeComponent();
    }

    private async void AccountBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            // 取消任何正在进行的动画
            _animationCts?.Cancel();
            // 为本次动画创建一个新取消令牌
            _animationCts = new CancellationTokenSource();
            

            if (_isTargetVisible)
            {
                _isTargetVisible = false;
                
                // 并行播放
                await Task.WhenAll(
                    _translateAnim.RunAsync(AccountBorder, _animationCts.Token),
                    _backAnimation.RunAsync(QrLoginBorder, _animationCts.Token)
                );
                
            }
            else
            {
                _isTargetVisible = true;
                
                await Task.WhenAll(
                    _translateAnim.RunAsync(QrLoginBorder, _animationCts.Token),
                    _backAnimation.RunAsync(AccountBorder, _animationCts.Token)
                );
                
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
    
    }
}