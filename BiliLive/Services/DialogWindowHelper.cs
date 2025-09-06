using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
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
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: not null } desktop)
        {
            var mainBorder = desktop.MainWindow.FindControl<Border>("MainBorder");
            var coverBorder = desktop.MainWindow.FindControl<Border>("Cover");
            if (mainBorder == null || coverBorder == null) return;
            
            var dialogWindow = new DialogWindow()
            {
                DataContext = new ErrorDialogWindowViewModel() { Message = message}
            }; 
            
            mainBorder.Effect = new BlurEffect{ Radius = 5 };
            coverBorder.IsVisible = true;
            
            await dialogWindow.ShowDialog(desktop.MainWindow);
        
            mainBorder.Effect = null;
            coverBorder.IsVisible = false;
            
        }
    }

}