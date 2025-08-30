using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BiliLive.Views.DialogWindow;

public partial class DialogWindow : Window
{
    public DialogWindow()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}