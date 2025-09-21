using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace BiliLive.Resources.Controls;

public partial class DialControl : UserControl
{
    private bool isDragging = false;
    private Point startPoint;
    private Point startOffset;

    public DialControl()
    {
        InitializeComponent();
   
    }

    
    
    
    
    
    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        // 检查鼠标左键是否按下
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            isDragging = true;
            // 获取鼠标在 Window 上的初始位置
            startPoint = e.GetPosition(this);
            // 获取 Border 相对于其父级 Canvas 的初始位置
            startOffset = new Point(Canvas.GetLeft(draggableBorder), Canvas.GetTop(draggableBorder));
            // 捕获鼠标，确保即使鼠标移出 Border 范围也能继续接收 PointerMoved 事件
            e.Pointer.Capture(draggableBorder);
        }
    }

    private void OnPointerMoved(object sender, PointerEventArgs e)
    {
        if (isDragging)
        {
            // 获取当前鼠标位置
            Point currentPoint = e.GetPosition(this);
            // 计算鼠标移动的距离
            Vector delta = currentPoint - startPoint;
                
            // 计算新的位置
            double newLeft = startOffset.X + delta.X;
            double newTop = startOffset.Y + delta.Y;
            
            
            // 限制拖拽范围在 Canvas 内部
            newLeft = Math.Max(0, Math.Min(newLeft, dragCanvas.Bounds.Width - draggableBorder.Bounds.Width));
            newTop = Math.Max(0, Math.Min(newTop, dragCanvas.Bounds.Height - draggableBorder.Bounds.Height));

            // 更新 Border 的位置
            Canvas.SetLeft(draggableBorder, newLeft);
            Canvas.SetTop(draggableBorder, newTop);
            
            // 更新 Clip 区域
            //100为textblock位置
            
            // 计算圆形裁剪区域
            double centerX = newLeft + draggableBorder.Bounds.Width / 2;
            double centerY = newTop + draggableBorder.Bounds.Height / 2;
            double radius = draggableBorder.Bounds.Width / 2; // 假设是圆形，半径是宽度的一半
        
            // 创建圆形几何图形
            var ellipseGeometry = new EllipseGeometry();
            ellipseGeometry.Center = new Point(centerX- 100, centerY -100);
            ellipseGeometry.RadiusX = radius;
            ellipseGeometry.RadiusY = radius;
        
            // 更新 TextBlock 的 Clip
            blackTextBlock.Clip = ellipseGeometry;
        
            Console.WriteLine($"Center: ({centerX}, {centerY}), Radius: {radius}");

        }
    }

    private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        isDragging = false;
        // 释放鼠标捕获
        e.Pointer.Capture(null);
    }
}