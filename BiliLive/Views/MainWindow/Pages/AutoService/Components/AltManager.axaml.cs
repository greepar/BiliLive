using Avalonia.Controls;
using Avalonia.Interactivity;


namespace BiliLive.Views.MainWindow.Pages.AutoService.Components;

public partial class AltManager : Window
{
    public AltManager()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button && DoneBtn.IsFocused)
        { }
        else
        {
            if (DataContext is AltManagerViewModel vm)
            {
                vm.AllowDoneClose = false;
            }
        }
        Close();
    }
}