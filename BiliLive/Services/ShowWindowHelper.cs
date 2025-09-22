using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Styling;
using BiliLive.Views.DialogWindow;

namespace BiliLive.Services;

public static class ShowWindowHelper
{
    public static async Task ShowWindowAsync(Window window)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktop)
        {
            var mainBorder = desktop.MainWindow.FindControl<Border>("MainBorder");
            var coverBorder = desktop.MainWindow.FindControl<Border>("CoverBorder");
            if (mainBorder == null || coverBorder == null) return;
            
            mainBorder.Effect = new BlurEffect{ Radius = 5 };
            await Task.WhenAll(
                 GenerateAnimation(true).RunAsync(coverBorder),
                 window.ShowDialog(desktop.MainWindow)
                );
            
            _ = GenerateAnimation(false).RunAsync(coverBorder);
            mainBorder.Effect = null;
            
            
        }
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