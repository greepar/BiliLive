using System;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using BiliLive.Core.Models.BiliService;
using BiliLive.Core.Services.BiliService;
using BiliLive.Views.AccountWindow;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliLive.Services;

public class AccountInterface()
{
    public async Task<LoginResult?> LoginAsync()
    {
        
        var child = new AccountWindow
        {
            DataContext = new AccountWindowViewModel()
        };

        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow is null)
        {
            return null;
        }
        var mainWindow = desktop.MainWindow;
        
        void OnMainWindowGotFocus(object? sender, EventArgs e)
        {
            child.Close();
            mainWindow.GotFocus -= OnMainWindowGotFocus;
        }

        mainWindow.GotFocus += OnMainWindowGotFocus;

        child.Show(mainWindow);
        return null;
    }
}