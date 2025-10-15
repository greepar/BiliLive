using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Core.Interface;
using BiliLive.Core.Models.BiliService;
using BiliLive.Resources;
using BiliLive.Services;
using BiliLive.Utils;
using BiliLive.Views.MainWindow.Pages.HomePage.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLive.Views.MainWindow.Pages.HomePage;

public enum CopyTarget
{
    RoomUrl,
    StreamKey,
    StreamUrl
}

public partial class HomeViewModel : ViewModelBase
{ 
    public const string LiveUrlFormat = "https://live.bilibili.com";
    private const string EmptyText = "未获取";
    
    [ObservableProperty] private string? _userName = "未登录";
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsRoomTitleChanged))] private long? _userId;
    [ObservableProperty] private long? _roomId;
    [ObservableProperty] private Bitmap? _userFace;


    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsRoomTitleChanged))]
    private string? _inputRoomTitle;

    private string? _roomTitle;
    public bool IsRoomTitleChanged => UserId != null && !string.IsNullOrWhiteSpace(InputRoomTitle)  && _roomTitle != InputRoomTitle;

    [ObservableProperty] private string? _roomArea = EmptyText;
    [ObservableProperty] private bool? _isFinishing;
    
    [ObservableProperty] private string _apiUrl = EmptyText;

    //直播Key相关
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(MaskedStreamKey))]  private string? _streamKey; 
    public string MaskedStreamKey =>
        string.IsNullOrEmpty(StreamKey)
            ? EmptyText
            : $"{StreamKey[..Math.Min(17, StreamKey.Length)]}**********{StreamKey[Math.Max(0, StreamKey.Length - 8)..]}";
    [ObservableProperty] private Bitmap? _roomCover;
    
    [ObservableProperty] private string _statusText = "未直播";
    
    [ObservableProperty] private int _views;
    [ObservableProperty] private int _danmakus;
    [ObservableProperty] private int _gifts;

    private readonly IBiliService _biliService;
    [ObservableProperty]private AreaSelectorViewModel _areaSelectorVm = new();
    public HomeViewModel(IServiceProvider? serviceProvider = null)
    {
        using var faceMs = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/userPic.jpg"));
        UserFace = PicHelper.ResizeStreamToBitmap(faceMs, 71 * 2, 71 * 2) ?? new Bitmap(faceMs);

        if (Design.IsDesignMode || serviceProvider == null)
        {
            _biliService = new BiliServiceImpl();
        }
        else
        {
            _biliService = serviceProvider.GetRequiredService<IBiliService>();
        }
    }


    public async Task LoadHomeVmAsync(LoginResult loginResult)
    {
        if (loginResult is LoginSuccess result)
        {
            //获取用户信息
            UserName = result.UserName;
            UserId = result.UserId;
            using var ms = new MemoryStream(result.UserFaceBytes);
            UserFace = PicHelper.ResizeStreamToBitmap(ms, 71 * 2, 71 * 2) ?? new Bitmap(ms);

            //获取直播间信息
            var roomInfo = await _biliService.GetRoomInfoAsync();
            RoomId = roomInfo.RoomId;
            RoomCover?.Dispose();
            using var rcMs = new MemoryStream(roomInfo.RoomCover);
            RoomCover = PicHelper.ResizeStreamToBitmap(rcMs, 132 * 2, 74 * 2) ?? new Bitmap(rcMs);
            _roomTitle = roomInfo.Title;
            InputRoomTitle = roomInfo.Title;
        }
    }

    [RelayCommand]
    private void SelectArea()
    {
        if (!_biliService.IsLogged) { return; }
        AreaSelectorVm.IsLogged = true;
        AreaSelectorVm.RefreshAreasCommand.Execute(null);
        if (AreaSelectorVm.BiliService != null) return;
        AreaSelectorVm.BiliService = _biliService;
        AreaSelectorVm.LoadAreasCommand.Execute(null);
    }
    
    [RelayCommand]
    private async Task OpenRoomUrlAsync()
    {
        if (RoomId == null) return;
        try
        { 
            BrowserUtil.OpenInBrowser($"{LiveUrlFormat}/{RoomId}");
        }
        catch (Exception ex)
        {
            await ShowWindowHelper.ShowErrorAsync("打开直播间失败:" + ex.Message);
        }
    }
    
    [RelayCommand]
    private async Task CopyTextAsync(CopyTarget target)
    {
        try
        {
            var typeName = target switch
            {
                CopyTarget.RoomUrl => "直播间链接",
                CopyTarget.StreamKey => "直播密钥",
                CopyTarget.StreamUrl => "推流地址",
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            } ;
            var text = target switch
            {
                CopyTarget.RoomUrl => RoomId == null ? string.Empty : $"{LiveUrlFormat}/{RoomId}",
                CopyTarget.StreamKey => StreamKey,
                CopyTarget.StreamUrl => ApiUrl,
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            };
            if (string.IsNullOrEmpty(text) || text.Equals(EmptyText)) return;
            var clipboard = ClipboardHelper.Get();
            await clipboard.SetTextAsync(text);
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"已复制{typeName}到剪贴板", Geometry.Parse(MdIcons.Check)));
        }
        catch (Exception ex)
        {
            await ShowWindowHelper.ShowErrorAsync("复制失败:" + ex.Message);
        }
    }

    [RelayCommand]
    private async Task ChangeRoomTitleAsync()
    {
        if (string.IsNullOrEmpty(InputRoomTitle)) return;
        try
        {
            await _biliService.ChangeRoomTitleAsync(InputRoomTitle);
        }
        catch (Exception ex)
        {
            await ShowWindowHelper.ShowErrorAsync("修改直播间标题失败:" + ex.Message);
            return;
        }
        WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("修改直播间标题成功", Geometry.Parse(MdIcons.Check)));
        _roomTitle = InputRoomTitle;
        OnPropertyChanged(nameof(IsRoomTitleChanged));
    }

    [RelayCommand]
    private async Task ChangeRoomAreaAsync()
    {
        try
        {
            await _biliService.ChangeRoomAreaAsync(231);
        }
        catch (Exception ex)
        {
            await ShowWindowHelper.ShowErrorAsync("修改直播间标题失败:" + ex.Message);
        }

        WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("修改直播间分区成功", Geometry.Parse(MdIcons.Check)));
    }
    
    public readonly CancellationTokenSource LiveDataCts = new();
    public async Task UpdateApiKeyAsync(string streamUrl, string streamKey, string liveKey)
    {
        StreamKey = streamKey;
        ApiUrl = streamUrl;

        var token = LiveDataCts.Token;
        while (!token.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(5000, token);
                var liveData = await _biliService.GetLiveDataAsync(liveKey);
                // {"code":0,"message":"0","ttl":1,"data":{"LiveTime":543,"AddFans":0,"HamsterRmb":0,"NewFansClub":0,"DanmuNum":0,"MaxOnline":2,"WatchedCount":1}}
                if (liveData.GetProperty("code").GetInt32() != 0)
                {
                    var msg = liveData.TryGetProperty("message", out var messageProp)
                        ? messageProp.GetString()
                        : "未知错误";
                    throw new Exception($"服务器返回错误: {msg}");
                }

                var data = liveData.GetProperty("data");
                Views = data.GetProperty("WatchedCount").GetInt32();
                Danmakus = data.GetProperty("DanmuNum").GetInt32();
                Gifts = data.GetProperty("NewFansClub").GetInt32();
            }
            catch (OperationCanceledException){break;}
            catch (Exception e)
            {
                await ShowWindowHelper.ShowErrorAsync("获取直播间信息失败:" + e.Message);
            }
        }
    }
}