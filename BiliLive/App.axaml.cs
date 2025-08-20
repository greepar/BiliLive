using System.Net.Http;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BiliLive.Views.MainWindow;
using BiliLive.Core.Services.BiliService;
using BiliLive.Services;
using BiliLive.Views.AccountWindow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BiliLive;

public class App : Application
{
    private IHost AppHost { get; set; } = null!;
    
    public override void Initialize()
    {
        var httpClient = new HttpClient();
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(httpClient);
                services.AddSingleton<LoginService>();
                services.AddSingleton<MainWindow>();
                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<AccountInterface>();
                services.AddTransient<AccountWindow>();
                services.AddTransient<AccountWindowViewModel>();
                
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