using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using BiliLive.Views.DialogWindow;

namespace BiliLive.Services;

public static class DialogWindowHelper
{
    public static async Task ShowDialogAsync(string message , string title = "Error")
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is not null)
        {
            var dialogWindow = new DialogWindow()
            {
                DataContext = new DialogWindowViewModel() { Message = message}
            }; 
            await dialogWindow.ShowDialog(desktop.MainWindow);
        }
    }

}