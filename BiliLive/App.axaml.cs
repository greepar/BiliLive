using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BiliLive.Core.Interface;
using BiliLive.Utils;
using BiliLive.Views.MainWindow;
using BiliLive.Views.MainWindow.Controls;
using BiliLive.Views.MainWindow.Pages.AutoService;
using BiliLive.Views.MainWindow.Pages.HomePage;
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
                services.AddTransient<MainWindowViewModel>();
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
            var mainWindowViewModel = AppHost.Services.GetRequiredService<MainWindowViewModel>();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
        }
        base.OnFrameworkInitializationCompleted();
    }
    
}