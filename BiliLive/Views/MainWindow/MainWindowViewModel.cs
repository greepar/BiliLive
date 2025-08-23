using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Core.Models.BiliService;
using BiliLive.Core.Services.BiliService;
using BiliLive.Models;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;
using Path = Avalonia.Controls.Shapes.Path;

namespace BiliLive.Views.MainWindow;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly BiliService? _biliService;
    private readonly LiveService? _liveService;
    private CancellationTokenSource _pollingCts = new ();
    
    //主窗口内容
    [ObservableProperty] private string? _userName = "Not Login";
    [ObservableProperty] private long? _userId = 196431435;
    [ObservableProperty] private Bitmap? _userFace ;
    
    [ObservableProperty] private string? _roomTitle = "123";
    [ObservableProperty] private string? _roomArea = "开播后获取..";
    [ObservableProperty] private string? _apiKey = "Null";
    [ObservableProperty] private string? _ffmpegPath;
    [ObservableProperty] private string? _videoPath;
    [ObservableProperty] private string? _status = "Not Login";
    [ObservableProperty] private string? _btnWord = "Start Stream";
    
    [ObservableProperty] private Bitmap? _roomCover ;
    [ObservableProperty] private bool _autoStart = false ;
    [ObservableProperty] private bool _isLoginOpen = false ;
    [ObservableProperty] private bool _checkTask = false ;
    [ObservableProperty] private bool _streamBtn = false ;
    
    //Popup登录窗口内容
    [ObservableProperty] private string _popupTitle = "Accounts";
    [ObservableProperty]private bool _inLogin = false;
    
    //Popup QR登录窗口内容
    [ObservableProperty] private bool _showCoverBox = false;
    [ObservableProperty] private Path _coverIcon;
    [ObservableProperty] private string _coverContent;
    [ObservableProperty] private bool _isScanned = false;
    [ObservableProperty] private bool _isConfirmed = false;
    [ObservableProperty] private bool _timeout = false;
    [ObservableProperty] private Bitmap _loginQrPic;
    [ObservableProperty] private int _loginProgressValue = 0;
    
    
    //临时确认窗口
    [ObservableProperty] private Bitmap _tempPic;
    [ObservableProperty] private string _tempUsername;
    [ObservableProperty] private long _tempUid;
    
    public MainWindowViewModel()
    {
        var coverStream = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/a.png"));
        var roomBm = PicHelper.ResizeStreamToBitmap(coverStream, 157, 89);
        RoomCover = roomBm;
        
        var stream = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/userPic.jpg"));
        var userPicBm = PicHelper.ResizeStreamToBitmap(stream, 47, 47);
        UserFace = userPicBm;
        
        LoadConfigCommand.Execute(null);
    }

    public MainWindowViewModel(BiliService biliService,LiveService liveService) : this()
    {
        _liveService = liveService;
        _biliService = biliService;
    }
    
    [RelayCommand]
    private async Task LoadConfigAsync()
    {
        var appConfig = await ConfigManager.LoadConfigAsync();
        if (appConfig == null)
        {
            return;
        }

        var loginResult = await _biliService!.LoginAsync(appConfig.BiliCookie);
        await ConfirmLoginAsync(loginResult);
    }
    
    [RelayCommand]
    private async Task LoginAsync()
    {
        await Task.Delay(1);
        if (_biliService==null) {return;}
        IsLoginOpen = !IsLoginOpen;
    }

    [RelayCommand]
    private async Task AddAccountAsync()
    {
        var loginInfo = await _biliService!.GetLoginUrlAsync();
        if (loginInfo == null){return;}
        
        //生成登录二维码
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(loginInfo.QrCodeUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);
        LoginQrPic = new Bitmap(new MemoryStream(qrCodeImage));
        
        //切换page
        PopupTitle = "Scan the QR to Login";
        InLogin = true;

        _pollingCts = new CancellationTokenSource();
        
        //开始轮巡qrCodeKey
        while (!_pollingCts.IsCancellationRequested)
        {
            try
            {
                var qrStatusCode = await _biliService.GeQrStatusCodeAsync(loginInfo.QrCodeKey);
                Console.WriteLine(qrStatusCode);
                await Task.Delay(1000, _pollingCts.Token);
                switch (qrStatusCode)
                {
                    case 86101:
                        //未扫码，继续等待
                        LoginProgressValue = 25;
                        Status = "Waiting";
                        break;
                    case 86090:
                        //已扫码，等待手机确认登录
                        Status = "Scanned";
                        LoginProgressValue = 50;
                        break;
                    case 0:
                        //登录成功
                        IsConfirmed = true;
                        LoginProgressValue = 100;
                        Status = "Login Success";
                        await _pollingCts.CancelAsync();
                        break;
                    case 86038:
                        //二维码失效
                        Status = "Expired.";
                        await _pollingCts.CancelAsync();
                        break;
                    default:
                        //未知的状态码
                        await _pollingCts.CancelAsync();
                        break;
                }
            }
            catch (Exception)
            {
                break;
            }
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await ConfigManager.SaveConfigAsync(ConfigType.BiliCookie, null);
        UserName = "Not Login";
        UserId = 196431435;
        RoomTitle = null;
        

    }
    
    //确认登录
    [RelayCommand]
    private async Task ConfirmLoginAsync(LoginResult? loginResult = null)
    {
        IsConfirmed = false;
        loginResult ??= await _biliService!.LoginAsync();
        if (loginResult is LoginSuccess result)
        {
            await ConfigManager.SaveConfigAsync(ConfigType.BiliCookie,result.BiliCookie);
            UserName = result.UserName;
            UserId = result.UserId;
            var faceBytes = result.UserFaceBytes;
            var stream = new MemoryStream(faceBytes);
            UserFace = PicHelper.ResizeStreamToBitmap(stream, 37, 37);

            var roomInfo = await _liveService!.GetRoomInfoAsync();
            var roomCover = roomInfo.RoomCover;
            var rcStream = new MemoryStream(roomCover);
            RoomCover = PicHelper.ResizeStreamToBitmap(rcStream, 157, 89);
            RoomTitle = roomInfo.Title;
        }
        else
        {
            //登录失败
            UserName = "Login Failed";
            UserId = null;
            var stream = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/UserPic.jpg"));
            UserFace = new Bitmap(stream);
        }
        
        //切换页面
        PopupTitle = "Accounts";
        InLogin = false;
        
        //取消轮巡
        await _pollingCts.CancelAsync();
    }
    //取消登录
    [RelayCommand]
    private async Task CancelLoginAsync()
    {
        await Task.Delay(1);
        InLogin = false;
        PopupTitle = "Accounts";
        await _pollingCts.CancelAsync();
    }
    
    [RelayCommand]
    private async Task StartServiceAsync()
    {
        await Task.Delay(1);
        InLogin = true;
        UserName = "Hello";
    }

    [RelayCommand]
    private async Task ChangeAreaAsync()
    {
        await Task.Delay(1);
        UserName = "HelloArea";
    }
  
}