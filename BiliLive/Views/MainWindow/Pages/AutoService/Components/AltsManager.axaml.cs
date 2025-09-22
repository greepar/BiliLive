using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace BiliLive.Views.MainWindow.Pages.AutoService.Components;

public partial class AltsManager : Window
{
    public AltsManager()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}