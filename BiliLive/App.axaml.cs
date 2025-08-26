using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BiliLive.Core.Interface;
using BiliLive.Core.Services.BiliService;
using BiliLive.Views.MainWindow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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