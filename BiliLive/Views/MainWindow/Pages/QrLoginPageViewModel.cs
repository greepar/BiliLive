using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BiliLive.Views.MainWindow.Pages;

public partial class QrLoginPageViewModel : ViewModelBase
{
    [ObservableProperty]private Bitmap _loginQrPic;
    [ObservableProperty]private bool _isScanned = false;

    public QrLoginPageViewModel()
    {
        var stream = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/qr.jpg"));
        LoginQrPic = new Bitmap(stream);
        
    }
}