using System;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Styling;

namespace BiliLive.Views.DialogWindow;

public partial class DialogWindow : Window
{
    
    public DialogWindow()
    {
        InitializeComponent();

        Opened += async (_, _) =>
        {
            await CreateAnimation(true).RunAsync(DialogMainBorder);
        };
        
    }

    private async void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await CreateAnimation(false).RunAsync(DialogMainBorder);
            Close();
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    private Animation CreateAnimation(bool isOpening)
    {
        return new Animation
        {
            Duration = TimeSpan.FromMilliseconds(150),
            Easing = new ExponentialEaseOut(),
            FillMode = FillMode.Forward,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(OpacityProperty, isOpening ? 1.0 : 0.0),
                        new Setter(ScaleTransform.ScaleXProperty, isOpening ? 1.0 : 0.9),
                        new Setter(ScaleTransform.ScaleYProperty, isOpening ? 1.0 : 0.9),
                    }
                }
            }
        };
    }


}