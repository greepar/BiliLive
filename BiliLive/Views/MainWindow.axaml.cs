using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace BiliLive.Views;

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
}