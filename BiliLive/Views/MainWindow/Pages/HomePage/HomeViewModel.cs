using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Core.Interface;
using BiliLive.Core.Models.BiliService;
using BiliLive.Core.Services.BiliService;
using BiliLive.Services;
using BiliLive.Utils;
using BiliLive.Views.MainWindow.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLive.Views.MainWindow.Pages.HomePage;

public partial class HomeViewModel : ViewModelBase
{
    [ObservableProperty] private object? _danmakuVm;
    
    [ObservableProperty] private string? _userName = "未登录";
    [ObservableProperty] private long? _userId = 196431435;
    [ObservableProperty] private Bitmap? _userFace ;
    
    [ObservableProperty] private string? _roomTitle;
    [ObservableProperty] private string? _roomArea = "开播后获取..";
    
    [ObservableProperty] private bool? _isFinishing = null;
    
    
    // private string? _apiKey = null;
    [ObservableProperty] private string _maskedApiKey = "Will be generated after start...";
    
    [ObservableProperty] private Bitmap? _roomCover ;
    
    [ObservableProperty] private string _statusText = "未直播";

    private readonly IBiliService _biliService;
    public HomeViewModel(IServiceProvider? serviceProvider = null)
    {
        using var faceMs = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/userPic.jpg"));
        UserFace = PicHelper.ResizeStreamToBitmap(faceMs, 71*2, 71*2) ?? new Bitmap(faceMs);
        
        if (Design.IsDesignMode || serviceProvider == null)
        {
            //设计时数据
            DanmakuVm = new DanmakuPanelViewModel();
            _biliService = new BiliServiceImpl();
        }
        else
        {
            DanmakuVm = serviceProvider.GetService(typeof(DanmakuPanelViewModel));
            _biliService = serviceProvider.GetRequiredService<IBiliService>();
        }
    }

    
    public async Task LoadHomeVmAsync(LoginResult loginResult)
    {
        if (loginResult is LoginSuccess result)
        {
            UserName = result.UserName;
            UserId = result.UserId;
            using var ms = new MemoryStream(result.UserFaceBytes);
            UserFace = PicHelper.ResizeStreamToBitmap(ms, 71*2, 71*2) ?? new Bitmap(ms);
            var roomInfo = await _biliService.GetRoomInfoAsync();
            RoomCover?.Dispose();
            using var rcMs = new MemoryStream(roomInfo.RoomCover);
            RoomCover = PicHelper.ResizeStreamToBitmap(rcMs, 132*2, 74*2) ?? new Bitmap(rcMs);
            RoomTitle = roomInfo.Title;
        }
    }
    
    [RelayCommand]
    private async Task SelectAreaAsync()
    {
        await ShowWindowHelper.ShowErrorAsync("hello");
    }
    
        // [RelayCommand]
    // private async Task CopyApiKeyToClipboard()
    // {
    //     if (string.IsNullOrWhiteSpace(_apiKey))
    //     {
    //         WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("请先开始直播",Geometry.Parse(MdIcons.Notice)));
    //         return;
    //     }
    //     var clipboard = ClipboardHelper.Get();
    //     WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("Copied to clipboard",Geometry.Parse(MdIcons.Check)));
    //     await clipboard.SetTextAsync(_apiKey);
    // }


    // [RelayCommand]
    // private async Task StartServiceAsync()
    // {
    //     _apiKey = await _biliService!.StartLiveAsync();
    //     
    //     if (_apiKey == null || _apiKey.Length <= 1 || _apiKey.StartsWith("Error"))
    //     {
    //         // if (_apiKey != null) await ShowWindowHelper.ShowDialogAsync(ShowWindowHelper.Status.Error, _apiKey);
    //         IsStreaming = false;
    //         // await DialogWindowHelper.ShowDialogAsync();
    //         return;
    //     }
    //     
    //
    //     
    //     MaskedApiKey = $"{_apiKey?.Substring(0, 17)}**********{_apiKey?.Substring(_apiKey.Length - 8)}";
    //
    //     //自动服务
    //     if (_asVm.IsEnabled)
    //     {
    //         if (string.IsNullOrWhiteSpace(_asVm.VideoPath) || string.IsNullOrWhiteSpace(_asVm.FfmpegPath))
    //         {
    //             WeakReferenceMessenger.Default.Send(
    //                 new ShowNotificationMessage("请先设置Ffmpeg和视频路径", Geometry.Parse(MdIcons.Notice))
    //             );
    //             return;
    //         }
    //
    //         var startResult = await FfmpegWrapper.StartStreamingAsync(
    //             _asVm.FfmpegPath!, _asVm.VideoPath!, "", _apiKey!
    //         );
    //
    //         if (!startResult)
    //         {
    //             // await ShowWindowHelper.ShowDialogAsync(
    //             //     ShowWindowHelper.Status.Error,
    //             //     "自动推流启动失败，请检查Ffmpeg和视频路径"
    //             // );
    //             IsStreaming = false;
    //             return;
    //         }
    //
    //
    //         IsStreaming = true;
    //         BtnWord = "Stop Stream";
    //         Status = "Streaming";
    //         WeakReferenceMessenger.Default.Send(
    //             new ShowNotificationMessage("自动推流已启动", Geometry.Parse(MdIcons.Check)));
    //     }
    //     WeakReferenceMessenger.Default.Send(
    //         new ShowNotificationMessage("开始推流成功", Geometry.Parse(MdIcons.Check))
    //     );
    // }
}