using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
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
        
        GenerateNumbers(BlackNumCanvas,Brushes.Black,12);
        GenerateNumbers(WhiteNumanvas,Brushes.White,12);
        // GenerateTicks();
    }

    
      private void GenerateNumbers(Canvas targetCanvas,IBrush brush,int ticks)
        {
            double radius = 120; // 数字距离圆心的距离
            double centerX = 150; // 圆心X坐标
            double centerY = 150; // 圆心Y坐标
            
            for (int i = 1; i <= ticks; i++)
            {
                // 计算每个数字的角度（以12点为起点）
                double angle = (i - 3) * 30 * Math.PI / 180; // 减去3以使12点在顶部
                
                // 计算数字位置
                double x = centerX + radius * Math.Cos(angle);
                double y = centerY + radius * Math.Sin(angle);
                
                // 创建数字文本
                var textBlock = new TextBlock
                {
                    Text = i.ToString(),
                    // Text = "1",
                    Foreground = brush,
                    FontSize = 18,
                    FontWeight = FontWeight.Bold
                };

                
                textBlock.Measure(Size.Infinity);

                // 2. 使用 DesiredSize 获取测量后的期望尺寸
                //    DesiredSize 是 Measure 过程的结果
                double textWidth = textBlock.DesiredSize.Width;
                double textHeight = textBlock.DesiredSize.Height;
        
                Console.WriteLine($"Number {i}: Desired Width = {textWidth}", $"Desired Height = {textHeight}");

                
                
                
                // 调整位置使文本居中
                Canvas.SetLeft(textBlock, x - textWidth / 2);
                Canvas.SetTop(textBlock, y - textHeight / 2);
                
                targetCanvas.Children.Add(textBlock);
                
            }
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
            
            // 计算圆形裁剪区域
            double centerX = (newLeft + draggableBorder.Bounds.Width / 2) ;
            double centerY = (newTop + draggableBorder.Bounds.Height / 2) ;
            double radius = draggableBorder.Bounds.Width / 2; // 假设是圆形，半径是宽度的一半
        
            // 创建圆形几何图形
            var ellipseGeometry = new EllipseGeometry();
            ellipseGeometry.Center = new Point(centerX , centerY );
            ellipseGeometry.RadiusX = radius;
            ellipseGeometry.RadiusY = radius;
        
            // 更新 TextBlock 的 Clip
            
            CalculateClip();
        }
    }

    private void OnPointerReleased(object sender, PointerReleasedEventArgs e)
    {
        isDragging = false;
        // 释放鼠标捕获
        e.Pointer.Capture(null);
    }
    
    
    private void CalculateTargetAngle()
    {
      
        
        // if (CurrentMode == EditMode.Hour)
        // {
        //     var hour = (int)Math.Round(angleDeg / 30.0);
        //     if (hour == 0) hour = 12;
        //     if (hour > 12) hour -= 12;
        //     DisplayHour = hour;
        // }
        // else if (CurrentMode == EditMode.Minute)
        // {
        //     var minute = (int)Math.Round(angleDeg / 6.0);
        //     if (minute >= 60) minute = 0;
        //     Minute = minute;
        // }
    }
    
    private void CalculateClip(double rotateAngle = 35)
    {
        
        //校准位置
        // Console.WriteLine(Stack1.Bounds.Width);
        // var positonA = Stack1.Bounds.Width / 2 ;
        // Canvas.SetLeft(Stack1,150 - positonA);
        
        // Angle.Angle = 0;
        
        
        rotateAngle = 80;
        
        var rotate = (RotateTransform)Stack1.RenderTransform;
        rotate.Angle = rotateAngle; // 触发动画
        
        double radians = rotateAngle * Math.PI / 180.0;

// 旋转中心
        double cx = 150;
        double cy = 150;

// 原始圆心
        double x = 150;
        double y = 150 + 103 + 19;
        
        double centerX = 150; ;
        double centerY = 150 + 103 + 19 ;
        double radius = 19; // 假设是圆形，半径是宽度的一半
       
        double newX = cx + (x - cx) * Math.Cos(radians) - (y - cy) * Math.Sin(radians);
        double newY = cy + (x - cx) * Math.Sin(radians) + (y - cy) * Math.Cos(radians);
        
        var ellipseGeometry = new EllipseGeometry();
        ellipseGeometry.Center = new Point(newX , newY );
        ellipseGeometry.RadiusX = radius;
        ellipseGeometry.RadiusY = radius;
        
        BlackNumCanvas.Clip = ellipseGeometry;
        
        Console.WriteLine($"Center: ({centerX}, {centerY}), Radius: {radius}");
    }
}