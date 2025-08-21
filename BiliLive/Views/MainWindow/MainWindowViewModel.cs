using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Core.Models.BiliService;
using BiliLive.Core.Services;
using BiliLive.Services;
using BiliLive.Views.MainWindow.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;
using Path = Avalonia.Controls.Shapes.Path;

namespace BiliLive.Views.MainWindow;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly BiliService? _biliService;
    private CancellationTokenSource _pollingCts = new ();
    
    //主窗口内容
    [ObservableProperty] private string? _userName = "Not Login";
    [ObservableProperty] private string? _roomTitle;
    [ObservableProperty] private string? _roomArea = "选择分区";
    [ObservableProperty] private string? _apiKey = "Null";
    [ObservableProperty] private string? _ffmpegPath;
    [ObservableProperty] private string? _videoPath;
    [ObservableProperty] private string? _status = "Not Login";
    [ObservableProperty] private string? _btnWord = "Start Stream";
    [ObservableProperty] private Bitmap? _userFace ;
    [ObservableProperty] private Bitmap? _roomCover ;
    [ObservableProperty] private bool _autoStart = false ;
    [ObservableProperty] private bool _isLoginOpen = false ;
    [ObservableProperty] private bool _checkTask = false ;
    [ObservableProperty] private bool _streamBtn = false ;
    [ObservableProperty] private long? _userId = 196431435;
    
    //Popup登录窗口内容
    [ObservableProperty] private string _popupTitle = "Accounts";
    [ObservableProperty]private object _currentPage = new AccountPageViewModel();
    [ObservableProperty]private bool _inLogin = false;
    
    //Popup QR登录窗口内容
    [ObservableProperty] private bool _showCoverBox = false;
    [ObservableProperty] private Path _coverIcon;
    [ObservableProperty] private string _coverContent;
    [ObservableProperty] private bool _isScanned = false;
    [ObservableProperty] private bool _timeout = false;
    [ObservableProperty] private Bitmap _loginQrPic;
    
    //临时确认窗口
    [ObservableProperty] private Bitmap _tempPic;
    [ObservableProperty] private string _tempUsername;
    [ObservableProperty] private long _tempUid;
    
    public MainWindowViewModel()
    {
        var stream = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/UserPic.jpg"));
        LoginQrPic = new Bitmap(stream);
    }

    public MainWindowViewModel(BiliService biliService) : this()
    {
        _biliService = biliService;
    }
    
    [RelayCommand]
    private async Task LoginAsync()
    {
        if (_biliService==null) {return;}
        IsLoginOpen = !IsLoginOpen;
    }

    [RelayCommand]
    private async Task AddAccountAsync()
    {
        var loginInfo = await _biliService.GetLoginUrlAsync();
        if (loginInfo == null){return;}
        
        //生成登录二维码
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(loginInfo.QrCodeUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);
        LoginQrPic = new Bitmap(new MemoryStream(qrCodeImage));
        
        //切换page
        CurrentPage = new QrLoginPageViewModel(); 
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
                        Status = "QR Code Canceled";
                        break;
                    case 86090:
                        //已扫码，等待手机确认登录
                        Status = "Scanned";
                        await _pollingCts.CancelAsync();
                        break;
                    case 0:
                        //登录成功
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
    //确认登录
    [RelayCommand]
    private async Task ConfirmLoginAsync()
    {
        if (_biliService == null) { return; }
        
        //获取登录状态
        
        //切换页面
        CurrentPage = new AccountPageViewModel();
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
        CurrentPage = new QrLoginPageViewModel(); 
        PopupTitle = "Accounts";
        await _pollingCts.CancelAsync();
    }
    
    [RelayCommand]
    private async Task StartServiceAsync()
    {
        CurrentPage = new AccountPageViewModel();
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