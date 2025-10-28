using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using BiliLive.Core.Interface;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using QRCoder;

namespace BiliLive.Views.MainWindow.Controls;


public partial class AccountsViewModel : ViewModelBase
{
    private readonly IBiliService? _biliService;
    private CancellationTokenSource _pollingCts = new ();
    
    public static GeneralState GeneralState => General.State;
    
    [ObservableProperty]private bool _isInLoginProcess;
    [ObservableProperty]private Bitmap? _userFace;
    [ObservableProperty]private Bitmap? _qrCodePic;

    public AccountsViewModel()
    {
        if (Design.IsDesignMode)
        {
            _biliService = new BiliServiceImpl();
        }

        General.State.PropertyChanged +=  (_, e) =>
        {
            if (e.PropertyName == nameof(General.State.UserFaceByte))
            {
                RefreshUserFaceAsync(General.State.UserFaceByte);
            }
        };
    }
    
    public AccountsViewModel(IBiliService biliService) : this()
    {
        _biliService = biliService;
    }

    private void RefreshUserFaceAsync(byte[]? userFaceByte)
    {
        //TODO: 异步处理
        try
        {
            if (userFaceByte == null || userFaceByte.Length == 0) { return; }
            using var ms = new MemoryStream(userFaceByte);
            UserFace = PicHelper.ResizeStreamToBitmap(ms,116,116);
        }
        catch (Exception)
        {
            // ignored
        }
    }
    
    [RelayCommand]
    private async Task LoginStatusConverter()
    {
        IsInLoginProcess = !IsInLoginProcess;
        if (IsInLoginProcess)
        {
            var qrInfo = await _biliService!.GetLoginUrlAsync();
            if (qrInfo == null){return;}

            //生成登录二维码
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(qrInfo.QrCodeUrl, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);
            using var stream = new MemoryStream(qrCodeImage);
            QrCodePic = new Bitmap(stream);
            
            _pollingCts = new CancellationTokenSource();

            while (!_pollingCts.IsCancellationRequested)
            {
                try
                {
                    var qrStatusCode = await _biliService.GeQrStatusCodeAsync(qrInfo.QrCodeKey);
                    await Task.Delay(1000, _pollingCts.Token);
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
                            var loginResult = await _biliService.LoginAsync();
                            WeakReferenceMessenger.Default.Send(new LoginMessage(loginResult));
                            await _pollingCts.CancelAsync();
                            break;
                        case 86038:
                            //二维码失效
                            // Status = "Expired.";
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
                finally
                {
                    // 释放旧的 Bitmap 资源
                    QrCodePic?.Dispose();
                }
            }
        }
    }

    [RelayCommand]
    private void CancelLogin()
    {
        IsInLoginProcess = !IsInLoginProcess;
        _pollingCts.Cancel();
        QrCodePic?.Dispose();
    }
    
}