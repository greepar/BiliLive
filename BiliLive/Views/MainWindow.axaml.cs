using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace BiliLive.Views.MainWindow;

public partial class MainWindow : Window
{
    // 账号窗口
    private CancellationTokenSource? _animationCts;

    private bool _isTargetVisible = true;
    private double _originalHeight;

    private double _originalWidth;

    //开始移动窗口
    private Point _startPoint = new(783, 455);

    public MainWindow()
    {
        InitializeComponent();

#if DEBUG
        // Topmost = true;
#endif
    }

    // 移动窗口
    private void MainWindowStartDragMove(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
    }

    private void MainWindowStartDragResize(object? sender, PointerPressedEventArgs e)
    {
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
        _originalWidth = MainBorder.Width;
        _originalHeight = MainBorder.Height;
        TotalBorder.PointerMoved += ResizeWindowMove;
        TotalBorder.PointerReleased += ResizeWindowRelease;
    }

    private void ResizeWindowRelease(object? sender, PointerReleasedEventArgs e)
    {
        var releasePoint = e.GetPosition(this);
        _startPoint = new Point(releasePoint.X > 783 ? releasePoint.X : 783,
            releasePoint.Y > 455 ? releasePoint.Y : 455);
        TotalBorder.PointerMoved -= ResizeWindowMove;
        TotalBorder.PointerReleased -= ResizeWindowRelease;
    }

    //窗口调整Move
    private void ResizeWindowMove(object? sender, PointerEventArgs e)
    {
        var position = e.GetPosition(MainBorder);
        // Console.WriteLine(  $"Move Positon: {position}, StartPositon: {_startPoint}");
        var deltaX = position.X - _startPoint.X;
        var deltaY = position.Y - _startPoint.Y;
        MainBorder.Width = double.Max(_originalWidth + deltaX, 800);
        Width = MainBorder.Width + 20;
        MainBorder.Height = double.Max(_originalHeight + deltaY, 470);
        Height = MainBorder.Height + 20;
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
}