using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Core.Interface;
using BiliLive.Core.Services.BiliService;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;

namespace BiliLive.Views.MainWindow.Controls;


public partial class AccountsViewModel : ViewModelBase
{
    private readonly IBiliService? _biliService;
    
    [ObservableProperty]private bool _isInLoginProcess;
    [ObservableProperty]private Bitmap? _qrCodePic;
    
    public AccountsViewModel()
    {
        if (Design.IsDesignMode)
        {
            _biliService = new BiliServiceImpl();
        }
    }
    
    public AccountsViewModel(IBiliService biliService) : this()
    {
        _biliService = biliService;
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

            
            // 释放旧的 Bitmap 资源
            QrCodePic?.Dispose();
            QrCodePic = new Bitmap(stream);
        }
    }
    
    
}