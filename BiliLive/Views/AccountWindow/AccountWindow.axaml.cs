using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Transformation;

namespace BiliLive.Views.AccountWindow;

public partial class AccountWindow : Window
{
    public AccountWindow()
    {
        InitializeComponent();
        //绑定动画
        Opened += (_, _) =>
        {
            MainBorder.Opacity = 1;
            MainBorder.RenderTransform = TransformOperations.Parse("scale(1)");
        };
    }
    

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}