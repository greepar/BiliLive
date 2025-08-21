using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Transformation;

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
        this.Topmost = true;
        // this.Topmost = false;
        // this.Activate(); 
        // this.Focus(); 
    }

    private void LoginPopup_OnOpened(object? sender, EventArgs e)
    {
            LoginBorder.Opacity = 1;
            LoginBorder.RenderTransform = new ScaleTransform(1, 1);
    }
    private async void LoginPopup_OnClosed(object? sender, EventArgs e)
    {
        // LoginBorder.RenderTransform = new ScaleTransform(0.8, 0.8);
        await Task.Delay(1);
        LoginBorder.Opacity = 0;
        // LoginBorder.RenderTransform = TransformOperations.Parse("scale(0.8)");


    }


    private async void LoginButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            LoginButton.IsEnabled = false;
            if (LoginPopup.IsOpen)
            {
                LoginBorder.Opacity = 0;
                await Task.Delay(300);
                LoginPopup.IsOpen = !LoginPopup.IsOpen;
            }
            else
            {
                LoginPopup.IsOpen = !LoginPopup.IsOpen;
                LoginBorder.Opacity = 1;
            }
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