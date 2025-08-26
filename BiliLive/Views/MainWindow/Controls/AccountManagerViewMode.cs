using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Core.Interface;
using BiliLive.Core.Models.BiliService;
using BiliLive.Core.Services.BiliService;
using BiliLive.Models;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;

namespace BiliLive.Views.MainWindow.Controls;

public partial class AccountManagerViewMode : ViewModelBase
{
    private CancellationTokenSource _pollingCts = new ();
    private readonly IBiliService? _biliService;
    
    [ObservableProperty] private string _title = "Accounts";
    [ObservableProperty]private bool _inLogin;

    //二维码相关信息
    [ObservableProperty] private string _status = "Accounts";
    [ObservableProperty] private int _loginProgressValue;
    [ObservableProperty] private Bitmap? _qrCodePic;
    [ObservableProperty] private bool _isScanned;
    [ObservableProperty] private bool _isConfirmed;

    //临时确认窗口
    [ObservableProperty] private Bitmap? _tempUserFace ;
    [ObservableProperty] private string? _tempUsername;
    [ObservableProperty] private string? _tempUid;
 

    //用户信息
    [ObservableProperty] private string? _userName;
    [ObservableProperty] private long? _userId;
    [ObservableProperty] private Bitmap? _userFace ;
    
    //无参构造函数，设计模式下使用
    public AccountManagerViewMode()
    {
        if (Design.IsDesignMode)
        {
            _biliService = new BiliServiceImpl();
        }
    }
    
    //构造函数注入biliService
    public AccountManagerViewMode(IBiliService biliService) : this()
    {
        _biliService = biliService;
    }
    
    
    //添加账号按钮
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
        QrCodePic = new Bitmap(new MemoryStream(qrCodeImage));
        
        //切换page
        Title = "Scan the QR to Login";
        InLogin = true;
        _pollingCts = new CancellationTokenSource();
        
        //开始轮巡qrCodeKey
        while (!_pollingCts.IsCancellationRequested)
        {
            try
            {
                var qrStatusCode = await _biliService.GeQrStatusCodeAsync(loginInfo.QrCodeKey);
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
                        await RefreshConfirmInfoAsync();
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
    
    //退出登录
    [RelayCommand]
    private async Task LogoutAsync()
    {
        await ConfigManager.SaveConfigAsync(ConfigType.BiliCookie, null);
        UserName = "Not Login";
        UserId = 196431435;
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

            var roomInfo = await _biliService!.GetRoomInfoAsync();
            var roomCover = roomInfo.RoomCover;
            var rcStream = new MemoryStream(roomCover);
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
        Title = "Accounts";
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
        Title = "Accounts";
        await _pollingCts.CancelAsync();
    }
    
    
    //刷新确认信息
    private async Task RefreshConfirmInfoAsync()
    {
        if (_biliService == null) { return; }
        var loginResult = await _biliService.LoginAsync();
        if (loginResult is LoginSuccess result)
        {
            TempUserFace = PicHelper.ResizeStreamToBitmap(new MemoryStream(result.UserFaceBytes), 54, 54);
            TempUsername = result.UserName;
            TempUid = result.UserId.ToString();
        }
    }
}