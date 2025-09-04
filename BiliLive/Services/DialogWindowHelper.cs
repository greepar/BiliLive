using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using BiliLive.Views.DialogWindow;

namespace BiliLive.Services;

public static class DialogWindowHelper
{
    public enum Status
    { 
        Error,Success
    }
    
    public static async Task ShowDialogAsync(Status status , string message)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is not null)
        {
            var dialogWindow = new DialogWindow()
            {
                DataContext = new ErrorDialogWindowViewModel() { Message = message}
            }; 
            await dialogWindow.ShowDialog(desktop.MainWindow);
        }
    }

}