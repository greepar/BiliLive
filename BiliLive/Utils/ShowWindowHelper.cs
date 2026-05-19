using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using BiliLive.Views.DialogWindow;
using QRCoder;

namespace BiliLive.Utils;

public static class ShowWindowHelper
{
    public static async Task ShowErrorAsync(string message)
    {
        var errorWindow = new DialogWindow
        {
            DataContext = new DialogWindowViewModel
            {
                Message = message
            }
        };
        await ShowWindowAsync(errorWindow);
    }
    
    public static async Task ShowQrCodeAsync(string qrCodeUrl)
    {
        //生成登录二维码
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrCodeUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);
        using var stream = new MemoryStream(qrCodeImage);
        
        using var vm = new QrDialogViewModel();
        vm.QrImage = new Bitmap(stream);
        var qrCodeWindow = new QrDialog
        {
            DataContext = vm
        };
        await ShowWindowAsync(qrCodeWindow);
    }
    
    
    public static async Task ShowWindowAsync(Window window)
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktop)
            return;

        var mainBorder = desktop.MainWindow.FindControl<Border>("MainBorder");
        var coverBorder = desktop.MainWindow.FindControl<Border>("CoverBorder");

        // 应用模糊效果（如果存在 MainBorder）
        if (mainBorder != null)
            mainBorder.Effect = new BlurEffect { Radius = 5 };

        // 同时跑遮罩动画（如果存在 CoverBorder）和模态对话框
        if (coverBorder != null)
        {
            await Task.WhenAll(
                GenerateAnimation(true).RunAsync(coverBorder),
                window.ShowDialog(desktop.MainWindow));

            _ = GenerateAnimation(false).RunAsync(coverBorder);
        }
        else
        {
            await window.ShowDialog(desktop.MainWindow);
        }

        if (mainBorder != null)
            mainBorder.Effect = null;
    }

    private static Animation GenerateAnimation(bool isOpening)
    {
        return new Animation()
        {
            Duration = TimeSpan.FromMilliseconds(300),
            Easing = new CubicEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, isOpening ? 0 : 0.2)

                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(Visual.OpacityProperty, isOpening ? 0.2 : 0)
                    }
                }
            }
        };
    }

}