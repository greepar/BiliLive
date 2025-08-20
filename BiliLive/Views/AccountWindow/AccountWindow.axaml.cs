using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Transformation;
using Microsoft.Extensions.DependencyInjection;

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
    
    public AccountWindow(IServiceProvider serviceProvider) : this()
    {
        var vm = serviceProvider.GetRequiredService<AccountWindowViewModel>();
        DataContext = vm;
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}