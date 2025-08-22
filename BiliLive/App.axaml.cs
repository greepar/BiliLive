using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BiliLive.Core.Services.BiliService;
using BiliLive.Models;
using BiliLive.Views.MainWindow;
using BiliLive.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BiliLive;

public class App : Application
{
   
    
    private IHost AppHost { get; set; } = null!;
    public override void Initialize()
    {
        //构造一个带有CookieContainer的HttpClient
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = cookieContainer
        };
        var httpClient = new HttpClient(handler);
        
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(httpClient);
                services.AddSingleton(cookieContainer);
                services.AddSingleton<BiliService>();
                services.AddSingleton<LiveService>();
                services.AddSingleton<MainWindow>();
                services.AddTransient<MainWindowViewModel>();
                services.AddSingleton<AccountInterface>();
                
                // 更多服务...
            })
            .Build();
        
        AvaloniaXamlLoader.Load(this);
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            //通过DI获取MainWindow
            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            var mainWindowViewModel = AppHost.Services.GetRequiredService<MainWindowViewModel>();
            mainWindow.DataContext = mainWindowViewModel;
            desktop.MainWindow = mainWindow;
        }
        base.OnFrameworkInitializationCompleted();
    }
    

}