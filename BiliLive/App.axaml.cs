using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using BiliLive.Core.Interface;
using BiliLive.Core.Services;
using BiliLive.Core.Services.BiliService;
using BiliLive.Utils;
using BiliLive.Views.MainWindow;
using BiliLive.Views.MainWindow.Controls;
using BiliLive.Views.MainWindow.Pages.AutoService;
using BiliLive.Views.MainWindow.Pages.HomePage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;

namespace BiliLive;

public class App : Application
{ 
    private IHost AppHost { get; set; } = null!;
    public override void Initialize()
    {
        //配置DI注入
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton<IBiliService,BiliServiceImpl>();
                services.AddSingleton<MainWindow>();
                services.AddTransient<MainWindowViewModel>();
                services.AddTransient<AutoServiceView>();
                services.AddTransient<AutoServiceViewModel>();
                services.AddTransient<AccountsViewModel>();
                services.AddTransient<HomeViewModel>();
               
                
                // 更多服务...
            })
            .Build();
        
        AvaloniaXamlLoader.Load(this);
        
#if DEBUG
        // this.AttachDeveloperTools();
#endif
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Exit += async (_, _) =>
            {
                await ConfigManager.ShutdownAsync();
            };
            
            //通过DI获取MainWindow
            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            var mainWindowViewModel = AppHost.Services.GetRequiredService<MainWindowViewModel>();
            mainWindow.DataContext = mainWindowViewModel;
            desktop.MainWindow = mainWindow;
        }
        base.OnFrameworkInitializationCompleted();
    }
    
}