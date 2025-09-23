
using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;


namespace BiliLive.Views.MainWindow.Pages.AutoService.Components;

public partial class AltsManagerViewModel : ViewModelBase
{
    private readonly GiftService? _giftService;

    [ObservableProperty] private Bitmap? _qrCodePic;
    
    [ObservableProperty] private string? _cookie;
    
    [ObservableProperty] private bool? _isSendGift;
    
    //proxy
    // [ObservableProperty] private ObservableCollection<AltAccount> _altAccounts = new();
    [ObservableProperty] private string? _proxyAddress;
    [ObservableProperty] private string? _proxyUsername;
    [ObservableProperty] private string? _proxyPassword;

    public AltsManagerViewModel()
    {
        if (Design.IsDesignMode)
        {
            _giftService = new GiftService("123","123");
        }

        var nullQrMs = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/nullQrCode.png"));
        QrCodePic = new Bitmap(nullQrMs);
        
    }
    public AltsManagerViewModel(GiftService giftService) : this()
    {
        _giftService = giftService;
    }

    private async Task QrLoginAsync()
    {
        // await _giftService.GetLoginUrlAsync();
    }
}
