using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Core.Interface;
using BiliLive.Core.Models.BiliService;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;


namespace BiliLive.Views.MainWindow.Pages.AutoService.Components;

//单个弹幕项
public partial class DanmakuItem(string text,Action<DanmakuItem> removeAction) : ObservableObject
{ 
    [ObservableProperty] private string _text = text; 
    [RelayCommand]private void Remove() => removeAction(this); 
}
public partial class AltManagerViewModel : ViewModelBase , IDisposable
{
    private readonly IAltService? _altService;
    [ObservableProperty] private ObservableCollection<DanmakuItem> _danmakuList = [];
    [ObservableProperty] private Bitmap? _qrCodePic;
    
    
    [ObservableProperty] private bool _allowDoneClose;
    [ObservableProperty] private string? _userName;
    [ObservableProperty] private string? _cookieValue;
    
    [ObservableProperty] private bool _isSendGift;
    
    //proxy 如果存在
    [ObservableProperty] private string? _proxyAddress;
    [ObservableProperty] private string? _proxyUsername;
    [ObservableProperty] private string? _proxyPassword;

    public AltManagerViewModel()
    {
        _altService = new AltServiceImpl();
        using var nullQrMs = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/nullQrCode.png"));
        QrCodePic = new Bitmap(nullQrMs);
    }
    
    //实际调用函数
    public AltManagerViewModel(bool isSettings = false) : this()
    {
        if (!isSettings)
        {
            GenerateQrCodeCommand.Execute(null);
        }
    }

    [RelayCommand]
    public void AddDanmaku(string text = "")
    {
        //重复检查
        if (DanmakuList.Any(item => item.Text == text)) { return; }
        DanmakuList.Add(new DanmakuItem(text,RemoveDanmaku));
    }
    private void RemoveDanmaku(DanmakuItem danmaku)
    {
        DanmakuList.Remove(danmaku);
    }
    
    [RelayCommand]
    private void EditCookie()
    {
        AllowDoneClose = false;
    }
    
    [RelayCommand]
    private async Task CheckCookieAsync()
    {
        if (CookieValue is null) return;
        using var tempAltService = new AltServiceImpl();
        try
        {
            var loginResult = await tempAltService.LoginAsync(CookieValue);
            if (loginResult is LoginSuccess result)
            {
                QrCodePic?.Dispose();
                using var ms = new MemoryStream(result.UserFaceBytes);
                QrCodePic = PicHelper.ResizeStreamToBitmap(ms,280,280);
            
                AllowDoneClose = true;
                //赋值Cookie
                CookieValue = result.BiliCookie;
                UserName = result.UserName;
            }else
            {
                //登录失败
                CookieValue = "登录失败，请检查Cookie是否正确";
            }
        }
        catch (Exception ex)
        {
            //登录失败
            CookieValue = $"登录失败，请检查Cookie是否正确[{ex.Message}]";
        }
    }
    
    [RelayCommand]
    private async Task GenerateQrCodeAsync()
    {
        var qrInfo = await _altService!.GetLoginUrlAsync();
        if (qrInfo is null) return;
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(qrInfo.QrCodeUrl, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeImage = qrCode.GetGraphic(20);
        using var ms = new MemoryStream(qrCodeImage);
        QrCodePic = new Bitmap(ms);
        //轮询等待扫码结果
        var pollingCts = new CancellationTokenSource();

        while (!pollingCts.IsCancellationRequested)
        {
            try
            {
                var qrStatusCode = await _altService.GeQrStatusCodeAsync(qrInfo.QrCodeKey);
                await Task.Delay(1000, pollingCts.Token);
                switch (qrStatusCode)
                {
                    case 86101:
                        //未扫码，继续等待
                        break;
                    case 86090:
                        //已扫码，等待手机确认登录
                        // Status = "Scanned";
                        // IsScanned = true;
                        // LoginProgressValue = 50;
                        break;
                    case 0:
                        //登录成功
                        AllowDoneClose = true;
                        var loginResult = await _altService.LoginAsync(null);

                        if (loginResult is LoginSuccess result)
                        {
                            //赋值Cookie
                            CookieValue = result.BiliCookie;
                            UserName = result.UserName;
                        }
                        
                        await pollingCts.CancelAsync();
                        break;
                    case 86038:
                        //二维码失效
                        // Status = "Expired.";
                        await pollingCts.CancelAsync();
                        break;
                    default:
                        //未知的状态码
                        await pollingCts.CancelAsync();
                        break;
                }
            }
            catch (Exception)
            {
                break;
            }
            finally
            {
                // 释放旧的 Bitmap 资源
                QrCodePic?.Dispose();
            }
        }
    }
    
    public void Dispose()
    {
        _altService?.Dispose();
        QrCodePic?.Dispose();
        QrCodePic = null;
        GC.SuppressFinalize(this);
    }
}
