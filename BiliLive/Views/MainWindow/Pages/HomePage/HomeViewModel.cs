using System;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Utils;
using BiliLive.Views.MainWindow.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BiliLive.Views.MainWindow.Pages.HomePage;

public partial class HomeViewModel : ViewModelBase
{
    [ObservableProperty] private object? _danmakuVm;
    
    [ObservableProperty] private string? _userName = "Not Login";
    [ObservableProperty] private long? _userId = 196431435;
    [ObservableProperty] private Bitmap? _userFace ;
    
    [ObservableProperty] private string? _roomTitle;
    [ObservableProperty] private string? _roomArea = "开播后获取..";
    
    // private string? _apiKey = null;
    [ObservableProperty] private string _maskedApiKey = "Will be generated after start...";
    
    [ObservableProperty] private Bitmap? _roomCover ;
    
    [ObservableProperty] private string _statusText = "未直播";

    public HomeViewModel()
    {
        if (Design.IsDesignMode)
        {
            var faceMs = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/userPic.jpg"));
            UserFace = PicHelper.ResizeStreamToBitmap(faceMs, 60, 60) ?? new Bitmap(faceMs);
        }
    }
    public HomeViewModel(IServiceProvider serviceProvider) : this()
    {
        DanmakuVm = serviceProvider.GetService(typeof(DanmakuPanelViewModel));
        var faceMs = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/userPic.jpg"));
        UserFace = PicHelper.ResizeStreamToBitmap(faceMs, 60, 60) ?? new Bitmap(faceMs);
    }
}