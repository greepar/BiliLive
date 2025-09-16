using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using BiliLive.Core.Interface;
using BiliLive.Core.Models.BiliService;
using BiliLive.Core.Services;
using BiliLive.Models;
using BiliLive.Resources;
using BiliLive.Services;
using BiliLive.Views.MainWindow.Controls;
using BiliLive.Views.MainWindow.Pages.About;
using BiliLive.Views.MainWindow.Pages.AutoService;
using BiliLive.Views.MainWindow.Pages.HomePage;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLive.Views.MainWindow;

//通知消息传递
public class ShowNotificationMessage(string value, Geometry geometry) 
    : ValueChangedMessage<string>(value)
{
    public Geometry Geometry { get; } = geometry;
}
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


public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IBiliService? _biliService;

    [ObservableProperty] private ObservableCollection<NotificationItem> _notifications =
    [
        //test icon
        // new("Welcome to BiliLive!", Geometry.Parse(MdIcons.Notice)),
        // new("Report issues on GitHub", Geometry.Parse(MdIcons.Error)),
        // new("Check for updates every week", Geometry.Parse(MdIcons.Check)),
    ];
    
    //构造子控件viewmodel
    [ObservableProperty] private AccountManagerViewMode _acVm;
    [ObservableProperty] private AccountsViewModel _accountVm = new ();
    [ObservableProperty] private AutoServiceViewModel _asVm = new ();
    [ObservableProperty] private DanmakuPanelViewModel _danmakuPanelVm = new ();
    private readonly HomeViewModel _homeVm = new ();
    
    [ObservableProperty] private object _currentVm = new AutoServiceViewModel();
    
    
    
    //主窗口内容
    [ObservableProperty] private string? _userName = "Not Login";
    [ObservableProperty] private long? _userId = 196431435;
    [ObservableProperty] private Bitmap? _userFace ;
    
    [ObservableProperty] private string? _roomTitle;
    [ObservableProperty] private string? _roomArea = "开播后获取..";
    
    private string? _apiKey;
    [ObservableProperty] private string _maskedApiKey = "Will be generated after start...";
    
    [ObservableProperty] private string? _status = "Not Login";
    
    
    [ObservableProperty] private bool _showWindow;

    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StreamButtonText))]
    private bool _isStreaming;
    public string StreamButtonText => IsStreaming ? "Stop stream" : "Start stream";

    
    [ObservableProperty] private string? _btnWord = "Start Stream";

    [ObservableProperty] private Bitmap? _roomCover ;
    
    public MainWindowViewModel(IServiceProvider? serviceProvider = null)
    {
        WeakReferenceMessenger.Default.Register<ShowNotificationMessage>(this,  (o, m) =>
        {
            var item = new NotificationItem(m.Value,m.Geometry);
            Notifications.Add(item);

            // 启动后台任务，5秒后移除
            _ =  DelayRemoveNotification(item);
        });

        
        var coverStream = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/a.png"));
        var roomBm = PicHelper.ResizeStreamToBitmap(coverStream, 314, 178);
        RoomCover = roomBm;
        
        var stream = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/userPic.jpg"));
        var userPicBm = PicHelper.ResizeStreamToBitmap(stream, 66, 66);
        UserFace = userPicBm;
        
        if (Design.IsDesignMode || serviceProvider == null)
        {
            // 设计时用默认实现
            AcVm = new AccountManagerViewMode();
        }
        else
        {
            _biliService = serviceProvider.GetRequiredService<IBiliService>();
            AcVm = serviceProvider.GetRequiredService<AccountManagerViewMode>();
            PreLoadCommand.Execute(null);
        }
    }
    
    
    //初始化内容
    [RelayCommand]
    private async Task PreLoadAsync()
    {
        LoadAcVm();
        var appConfig = await ConfigManager.LoadConfigAsync();
        if (appConfig == null) { return; }
        
        //初始化AutoService配置
        AsVm.VideoPath = appConfig.VideoPath;
        AsVm.FfmpegPath = appConfig.FfmpegPath;
        AsVm.IsEnabled = appConfig.EnableAutoService;
        AsVm.AsHeight = AsVm.IsEnabled ? 140 : 35;
        AsVm.AutoStart = appConfig.AutoStart;
        AsVm.Check60MinTask = appConfig.Check60MinTask;
       
        //监测Cookie是否存在
        if (string.IsNullOrWhiteSpace(appConfig.BiliCookie)) { return; }
        var loginResult = await _biliService!.LoginAsync(appConfig.BiliCookie);
        
        await LoadLoginResult(loginResult);
        //检查是否需要自动开播
        if (AsVm is { AutoStart: true, IsEnabled: true } && loginResult is LoginSuccess)
        {
            await StartServiceAsync();
        }
    }


    
    //登录相关
    [RelayCommand]
    private async Task LoginAsync()
    {
        await Task.Delay(1);
        ShowWindow = !ShowWindow;
        // if (_biliService==null) {return;}
        // AcVm.ShowWindow = !AcVm.ShowWindow;
    }
    

    
    //功能
    [RelayCommand]
    private async Task ChangeAreaAsync()
    {
        await Task.Delay(1);
        UserName = "HelloArea";
    }
    
    [RelayCommand]
    private async Task CopyApiKeyToClipboard()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("请先开始直播",Geometry.Parse(MdIcons.Notice)));
            return;
        }
        var clipboard = ClipboardHelper.Get();
        WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("Copied to clipboard",Geometry.Parse(MdIcons.Check)));
        await clipboard.SetTextAsync(_apiKey);
    }


    [RelayCommand]
    private async Task StartServiceAsync()
    {
        _apiKey = await _biliService!.StartLiveAsync();
        
        if (_apiKey == null || _apiKey.Length <= 1 || _apiKey.StartsWith("Error"))
        {
            if (_apiKey != null) await DialogWindowHelper.ShowDialogAsync(DialogWindowHelper.Status.Error, _apiKey);
            IsStreaming = false;
            // await DialogWindowHelper.ShowDialogAsync();
            return;
        }
        
    
        
        MaskedApiKey = $"{_apiKey?.Substring(0, 17)}**********{_apiKey?.Substring(_apiKey.Length - 8)}";

        //自动服务
        if (AsVm.IsEnabled)
        {
            if (string.IsNullOrWhiteSpace(AsVm.VideoPath) || string.IsNullOrWhiteSpace(AsVm.FfmpegPath))
            {
                WeakReferenceMessenger.Default.Send(
                    new ShowNotificationMessage("请先设置Ffmpeg和视频路径", Geometry.Parse(MdIcons.Notice))
                );
                return;
            }

            var startResult = await FfmpegWrapper.StartStreamingAsync(
                AsVm.FfmpegPath!, AsVm.VideoPath!, "", _apiKey!
            );

            if (!startResult)
            {
                await DialogWindowHelper.ShowDialogAsync(
                    DialogWindowHelper.Status.Error,
                    "自动推流启动失败，请检查Ffmpeg和视频路径"
                );
                IsStreaming = false;
                return;
            }


            IsStreaming = true;
            BtnWord = "Stop Stream";
            Status = "Streaming";
            WeakReferenceMessenger.Default.Send(
                new ShowNotificationMessage("自动推流已启动", Geometry.Parse(MdIcons.Check)));
        }
        WeakReferenceMessenger.Default.Send(
            new ShowNotificationMessage("开始推流成功", Geometry.Parse(MdIcons.Check))
        );
    }

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

    [RelayCommand]
    private async Task UpdateCoverAsync()
    {
        //选择图片
        var imagePath = await FolderPickHelper.PickFileAsync("Choose an image", [".png", ".jpg", ".jpeg"]);
        WeakReferenceMessenger.Default.Send(
            new ShowNotificationMessage($"封面目录 {imagePath}", Geometry.Parse(MdIcons.Check)));
    }
    
    private async Task DelayRemoveNotification(NotificationItem item)
    {
       await Task.Delay(3000);
         Notifications.Remove(item);
    }
    
    private async Task LoadLoginResult(LoginResult loginResult)
    {
        if (loginResult is LoginSuccess result)
        {
            await ConfigManager.SaveConfigAsync(ConfigType.BiliCookie,result.BiliCookie);
            UserName = result.UserName;
            UserId = result.UserId;
            var faceBytes = result.UserFaceBytes;
            var stream = new MemoryStream(faceBytes);
            UserFace = PicHelper.ResizeStreamToBitmap(stream, 37, 37);

            var roomInfo = await _biliService!.GetRoomInfoAsync();
            var roomCover = roomInfo.RoomCover;
            var rcStream = new MemoryStream(roomCover);
            RoomTitle = roomInfo.Title;
            RoomCover = PicHelper.ResizeStreamToBitmap(rcStream, 314, 178);
            LoadAcVm();
        }
        else
        {
            //登录失败
            UserName = "Login Failed";
            UserId = null;
            var stream = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/UserPic.jpg"));
            UserFace = new Bitmap(stream);
        }
    }
    
    private void LoadAcVm()
    {
        //初始化账号面板
        AcVm.UserName = UserName;
        AcVm.UserFace = UserFace;
        AcVm.UserId = UserId;
    }
    
    
    [RelayCommand]
    private void SwitchAbout()
    {
        // if (_biliService==null) {return;}
        CurrentVm = new AboutView();
    }
    
    [RelayCommand]
    private void SwitchAutoService()
    {
        // if (_biliService==null) {return;}
        CurrentVm = AsVm;
    }    
    
    [RelayCommand]
    private void SwitchHomePage()
    {
        // if (_biliService==null) {return;}
        CurrentVm = _homeVm;
    }
    
    
}