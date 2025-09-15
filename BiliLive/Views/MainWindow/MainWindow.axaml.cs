using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace BiliLive.Views.MainWindow;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

#if DEBUG
        // Topmost = true;
#endif
    }


    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
    }

    private void CloseButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MinimizeButton_OnClick(object? sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }


    // private async void LoginButton_OnClick(object? sender, RoutedEventArgs e)
    // {
    //     try
    //     {
    //         LoginButton.IsEnabled = false;
    //         if (LoginBorder.IsVisible)
    //         {
    //             LoginBorder.Opacity = 0;
    //             await Task.Delay(300);
    //             LoginBorder.IsVisible = !LoginBorder.IsVisible;
    //         }
    //         else
    //         {
    //             LoginBorder.IsVisible = !LoginBorder.IsVisible;
    //             LoginBorder.Opacity = 1;
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Error in LoginButton_OnClick: {ex.Message}");
    //     }
    //     finally
    //     {
    //         LoginButton.IsEnabled = true;
    //     }
    // }


    // private void CoverBorder_OnPointerEntered(object? sender, PointerEventArgs e)
    // {
    //     CoverBar.Height = 20;
    // }
    //
    // private void CoverBorder_OnPointerExited(object? sender, PointerEventArgs e)
    // {
    //     CoverBar.Height = 0;
    // }

    private void AccountBtn_OnClick(object? sender, RoutedEventArgs e)
    {
        
    }
}