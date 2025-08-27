using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Core.Models.BiliService;
using BiliLive.Models;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;
using BiliLive.Core.Interface;
using BiliLive.Services;
using BiliLive.Views.MainWindow.Controls;
using Path = Avalonia.Controls.Shapes.Path;

namespace BiliLive.Views.MainWindow;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IBiliService? _biliService;
    
    //构造QR轮询CTS
    
    //构造子控件viewmodel
    [ObservableProperty] private AutoServiceViewModel _asVm = new ();
    [ObservableProperty] private DanmakuPanelViewModel _danmakuPanelVm = new ();
    
    
    //主窗口内容
    [ObservableProperty] private string? _userName = "Not Login";
    [ObservableProperty] private long? _userId = 196431435;
    [ObservableProperty] private Bitmap? _userFace ;
    
    [ObservableProperty] private string? _roomTitle;
    [ObservableProperty] private string? _roomArea = "开播后获取..";
    
    private string? _apiKey;
    [ObservableProperty] private string _maskedApiKey = "Will be generated after start start...";

    [ObservableProperty] private string? _ffmpegPath;
    [ObservableProperty] private string? _videoPath;
    [ObservableProperty] private string? _status = "Not Login";

    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StreamButtonText))]
    private bool _isStreaming;
    public string StreamButtonText => IsStreaming ? "Stop stream" : "Start stream";

    
    [ObservableProperty] private string? _btnWord = "Start Stream";

    [ObservableProperty] private Bitmap? _roomCover ;
    [ObservableProperty] private bool _autoStart;
    [ObservableProperty] private bool _checkTask;
    [ObservableProperty] private bool _isLoginOpen ;
    [ObservableProperty] private bool _streamBtn;
    
    
    //Popup QR登录窗口内容
    [ObservableProperty] private bool _showCoverBox;
    [ObservableProperty] private Path? _coverIcon;
    [ObservableProperty] private string? _coverContent;
    [ObservableProperty] private bool _isScanned;
    [ObservableProperty] private bool _isConfirmed;
    [ObservableProperty] private bool _timeout;
    [ObservableProperty] private Bitmap? _loginQrPic;
    [ObservableProperty] private int _loginProgressValue;
    
    
    //临时确认窗口
    [ObservableProperty] private Bitmap? _tempPic;
    [ObservableProperty] private string? _tempUsername;
    [ObservableProperty] private long _tempUid;
    
    public MainWindowViewModel()
    {
        var coverStream = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/a.png"));
        var roomBm = PicHelper.ResizeStreamToBitmap(coverStream, 314, 178);
        RoomCover = roomBm;
        
        var stream = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/userPic.jpg"));
        var userPicBm = PicHelper.ResizeStreamToBitmap(stream, 47, 47);
        UserFace = userPicBm;
        
        PreLoadCommand.Execute(null);
    }

    public MainWindowViewModel(IBiliService biliService) : this()
    {
        _biliService = biliService;
    }
    
    //初始化内容
    [RelayCommand]
    private async Task PreLoadAsync()
    {
        var appConfig = await ConfigManager.LoadConfigAsync();
        if (appConfig == null || string.IsNullOrWhiteSpace(appConfig.BiliCookie))
        {
            return;
        }

        //初始化AutoService配置
        AsVm.VideoPath = appConfig.VideoPath;
        AsVm.FfmpegPath = appConfig.FfmpegPath;
        AsVm.ShowOptions = appConfig.ShowAsOption;
        AsVm.AutoStart = appConfig.AutoStart;
        AsVm.Check60MinTask = appConfig.Check60MinTask;

        
        var loginResult = await _biliService!.LoginAsync(appConfig.BiliCookie);
        // await ConfirmLoginAsync(loginResult);
    }
    
    
    //登录相关
    [RelayCommand]
    private async Task LoginAsync()
    {
        await Task.Delay(1);
        if (_biliService==null) {return;}
        IsLoginOpen = !IsLoginOpen;
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
        var clipboard = ClipboardHelper.Get();
        await clipboard.SetTextAsync(_apiKey);
    }
    
    
    [RelayCommand]
    private async Task StartServiceAsync()
    {
        _apiKey = await _biliService!.StartLiveAsync();
        if (_apiKey == null || _apiKey.Length <=1)
        {
            MaskedApiKey = "「获取失败，请检查登录状态」";
            return;
        }
        MaskedApiKey = _apiKey.StartsWith("错误") ? _apiKey : $"{_apiKey?.Substring(0, 17)}**********{_apiKey?.Substring(_apiKey.Length - 8)}";
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
}