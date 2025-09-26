using Avalonia.Controls;
using Avalonia.Interactivity;


namespace BiliLive.Views.MainWindow.Pages.AutoService.Components;

public partial class AltsManager : Window
{
    public AltsManager()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        
        if (sender is Button && DoneBtn.IsFocused)
        {
            if (DataContext is AltsManagerViewModel vm)
            {
                vm.SaveExitCommand.Execute(null);
                if (!vm.AllowDoneClose)
                {
                    return;
                }
            }
        }
        Close();
    }
}