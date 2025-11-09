using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BiliLive.Core.Interface;
using BiliLive.Core.Models.BiliService;
using BiliLive.Models;
using BiliLive.Resources;
using BiliLive.Views.MainWindow.Controls;
using BiliLive.Views.MainWindow.Pages.About;
using BiliLive.Views.MainWindow.Pages.AutoService;
using BiliLive.Views.MainWindow.Pages.HomePage;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLive.Views.MainWindow;

//传递登录信息
public class ShowNotificationMessage(string text, Geometry geometry) 
{
    public string Message { get; } = text;
    public Geometry Geometry { get; } = geometry;
}

public class LoginMessage(LoginResult loginResult) 
    : ValueChangedMessage<LoginResult>(loginResult){}    


//单个通知项
public partial class NotificationItem : ObservableObject
{
    [ObservableProperty] private string _message;
    [ObservableProperty] private Geometry _geometry;
    public NotificationItem(string msg, Geometry geometry)
    {
        Geometry = geometry;
        Message = msg;
    }
}

//导航页面
public enum NavigationPage
{
    Home,
    AutoService,
    About
}

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IBiliService _biliService;
    private DispatcherTimer? _minuteTimer;
    public static GeneralState GeneralState => General.State;
    
    [ObservableProperty] private ObservableCollection<NotificationItem> _notifications =
    [
        //图标列表
        // new("Welcome to BiliLive!", Geometry.Parse(MdIcons.Notice)),
        // new("Report issues on GitHub", Geometry.Parse(MdIcons.Error)),
        // new("Check for updates every week", Geometry.Parse(MdIcons.Check)),
    ];
    
    //构造子控件viewmodel
    [ObservableProperty] private AccountsViewModel _accountVm;
    private readonly AutoServiceViewModel _asVm;
    private readonly HomeViewModel _homeVm;
    
    //设置默认页面
    [ObservableProperty]private NavigationPage _currentBtn = NavigationPage.Home;
    [ObservableProperty] private object _currentVm;
    
    //主窗口内容
    [ObservableProperty] private string? _currentTime = "01:19" ;
    [ObservableProperty] private Bitmap? _userFace ;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StreamButtonText))]
    private bool _isStreaming;
    public string StreamButtonText => IsStreaming ? "Stop stream" : "Start stream";
    public Geometry StreamButtonIcon => IsStreaming ? Geometry.Parse(MdIcons.Restart) : Geometry.Parse(MdIcons.Start);
    
    
    public MainWindowViewModel(IServiceProvider? serviceProvider = null)
    {
        //传入服务
        WeakReferenceMessenger.Default.Register<ShowNotificationMessage>(this,  (o, m) =>
        {
            var item = new NotificationItem(m.Message,m.Geometry);
            Notifications.Add(item);
            // 移除重复内容
            // if (Notifications.All(x => x.Message != m.Value))
            // {
            //     Notifications.Add(item);
            // }
            _ =  DelayRemoveNotification(item);
        });
        WeakReferenceMessenger.Default.Register<LoginMessage>(this,  (o, m)  => 
        {
            _ = LoadLoginResult(m.Value,true);
        });
        
        using var stream = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/userPic.jpg"));
        UserFace = PicHelper.ResizeStreamToBitmap(stream, 66, 66);
        
        if (Design.IsDesignMode || serviceProvider == null)
        {
            // 设计时用默认实现
            _biliService = new BiliServiceImpl();
            AccountVm = new AccountsViewModel();
            _asVm = new AutoServiceViewModel();
            _homeVm = new HomeViewModel();
        }
        else
        {
            _biliService = serviceProvider.GetRequiredService<IBiliService>();
            AccountVm = serviceProvider.GetRequiredService<AccountsViewModel>();
            _asVm = serviceProvider.GetRequiredService<AutoServiceViewModel>();
            _homeVm = serviceProvider.GetRequiredService<HomeViewModel>();
            LoadAccountCommand.Execute(null);
        }
        CurrentVm = _homeVm;
        
        General.State.PropertyChanged +=  (_, e) =>
        {
            if (e.PropertyName == nameof(General.State.UserFaceByte))
            {
                RefreshUserFaceAsync(General.State.UserFaceByte);
            }
        };
    }
    
    private void RefreshUserFaceAsync(byte[]? userFaceByte)
    {
        try
        {
            if (userFaceByte == null || userFaceByte.Length == 0) { return; }
            using var ms = new MemoryStream(userFaceByte);
            UserFace?.Dispose();
            UserFace = PicHelper.ResizeStreamToBitmap(ms,116,116);
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    //初始化内容
    [RelayCommand]
    private async Task LoadAccountAsync()
    {
        var appConfig = await ConfigManager.LoadConfigAsync();
        
        //初始化AutoService配置
        await _asVm.InitializeCommand.ExecuteAsync(null);
        //启动时间更新服务
        StartUpdateTimeService();
        
        //监测Cookie是否存在
        if (string.IsNullOrWhiteSpace(appConfig.BiliCookie)) { return; }
        var loginResult = await _biliService.LoginAsync(appConfig.BiliCookie);
        
        await LoadLoginResult(loginResult);
    }

#if DEBUG
    // 打开当前程序文件夹，仅调试模式下存在
    [RelayCommand]
    private void OpenCurrentFolder()
    {
        var currentPath = AppDomain.CurrentDomain.BaseDirectory;
        Process.Start(new ProcessStartInfo
        {
            FileName = currentPath,
            UseShellExecute = true
        });
    }
#endif
    
    private bool _isDarkTheme = true;
    [RelayCommand]
    private void SwitchTheme()
    {
        _isDarkTheme = !_isDarkTheme;
        var themeVariant = _isDarkTheme ? ThemeVariant.Light : ThemeVariant.Dark;
        AvaloniaUtils.SwitchTheme(themeVariant);
    }
    
    [RelayCommand]
    private void StartUpdateTimeService() 
    {
        CurrentTime = DateTime.Now.ToString("HH:mm");
        
        // 计算到下一分钟整点的时间
        var now = DateTime.Now;
        var secondsToNextMinute = 60 - now.Second;
        
        Task.Delay(TimeSpan.FromSeconds(secondsToNextMinute)).ContinueWith(_ =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                // 创建每分钟刷新一次的定时器
                _minuteTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMinutes(1)
                };
                _minuteTimer.Tick += (_, _) => 
                {
                    CurrentTime = DateTime.Now.ToString("HH:mm");
                };
                _minuteTimer.Start();
            
                // 立即更新一次（整点时刻）
                CurrentTime = DateTime.Now.ToString("HH:mm");
            });
        });
    }
    
    private async Task DelayRemoveNotification(NotificationItem item)
    {
       await Task.Delay(3000);
         Notifications.Remove(item);
    }
    
    private async Task LoadLoginResult(LoginResult loginResult,bool saveCookie = false)
    {
        if (loginResult is LoginSuccess result)
        {
            if (saveCookie) await ConfigManager.SaveConfigAsync(ConfigType.BiliCookie,result.BiliCookie); 
           
            GeneralState.UserName = result.UserName;
            GeneralState.UserId = result.UserId;
            GeneralState.IsLogin = true;
            
            var faceBytes = result.UserFaceBytes;
            using var stream = new MemoryStream(faceBytes);
            UserFace?.Dispose();
            UserFace = PicHelper.ResizeStreamToBitmap(stream, 66, 66);
            
            //刷新HomeView
            await _homeVm.LoadHomeVmAsync(loginResult);
        }
        else
        {
            //登录失败
            GeneralState.UserName = "Login Failed";
            var stream = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/UserPic.jpg"));
            UserFace?.Dispose();
            UserFace = PicHelper.ResizeStreamToBitmap(stream, 66, 66);
        }
    }
    
    
    [RelayCommand]
    private void SwitchPage(NavigationPage navigationPage)
    {
        if (CurrentBtn == navigationPage)return;
        switch (navigationPage)
        {
            case NavigationPage.Home:
                CurrentBtn = NavigationPage.Home;
                CurrentVm = _homeVm;
                break;
            case NavigationPage.AutoService:
                CurrentBtn = NavigationPage.AutoService;
                CurrentVm = _asVm;
                break;
            case NavigationPage.About:
                CurrentBtn = NavigationPage.About;
                CurrentVm = new AboutViewModel();
                break;
        }
    }

    [RelayCommand]
    private async Task StartMainService()
    {
        try
        {
            if (!IsStreaming)
            {
                if (General.State.IsStreaming)
                {
                    WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("当前直播被自动直播占用，请先暂停自动直播。"
                        ,Geometry.Parse(MdIcons.Error)));
                    return;
                }
                var a = await _biliService.StartLiveAsync();
                // if (responseCode == 60024) return "Error-当前账号在触发风控，无法开播，尝试手机开播一次后再使用本软件开播";
                var streamUrl = a.GetProperty("data").GetProperty("rtmp").GetProperty("addr").GetString();
                var streamKey = a.GetProperty("data").GetProperty("rtmp").GetProperty("code").GetString();
                var liveKey = a.GetProperty("data").GetProperty("live_key").GetString();

                if (string.IsNullOrWhiteSpace(streamKey) || string.IsNullOrWhiteSpace(streamUrl) ||
                    string.IsNullOrWhiteSpace(liveKey))
                {
                    var code = a.GetProperty("code").GetInt32();
                    var errMsg = a.GetProperty("message").GetString();
                    await ShowWindowHelper.ShowErrorAsync($"开始推流失败\n,错误码:{code},错误信息:{errMsg}");
                    throw new Exception($"开始推流失败\n,错误码:{code},错误信息:{errMsg}");
                }

                WeakReferenceMessenger.Default.Send(
                    new ShowNotificationMessage("开启推流成功", Geometry.Parse(MdIcons.Check)));
                WeakReferenceMessenger.Default.Send(new StartRefreshLiveInfoMessage(streamUrl, streamKey, liveKey));
            }
            else
            {
                WeakReferenceMessenger.Default.Send(new StopRefreshLiveInfoMessage());
                await _biliService.StopLiveAsync();
                WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("已停止推流",
                    Geometry.Parse(MdIcons.Check)));
            }
        }
        catch (Exception ex)
        {
            await ShowWindowHelper.ShowErrorAsync("启动推流失败:" + ex.Message);
        }
        IsStreaming = !IsStreaming;
        General.State.IsStreaming = IsStreaming;
        
    }
    
}