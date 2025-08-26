using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace BiliLive.Views.MainWindow;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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

    private void InputElement_OnGotFocus(object? sender, GotFocusEventArgs e)
    {
        Topmost = true;
        // this.Topmost = false;
        // this.Activate(); 
        // this.Focus(); 
    }
    

    private async void LoginButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await Task.Delay(1);
            LoginButton.IsEnabled = false;
            // if (LoginBorder.IsVisible)
            // {
            //     LoginBorder.Opacity = 0;
            //     await Task.Delay(300);
            //     LoginBorder.IsVisible = !LoginBorder.IsVisible;
            // }
            // else
            // {
            //     LoginBorder.IsVisible = !LoginBorder.IsVisible;
            //     LoginBorder.Opacity = 1;
            // }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in LoginButton_OnClick: {ex.Message}");
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }
}