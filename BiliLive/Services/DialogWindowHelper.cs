using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using BiliLive.Views.DialogWindow;
using BiliLive.Views.ErrorWindow;
using BiliLive.Views.MainWindow;

namespace BiliLive.Services;

public static class DialogWindowHelper
{
    public static async Task ShowDialogAsync()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
            desktop.MainWindow is not null)
        {
            var dialogWindow = new DialogWindow()
            {
                
            }; 
            await dialogWindow.ShowDialog(desktop.MainWindow);
        }
    }

}