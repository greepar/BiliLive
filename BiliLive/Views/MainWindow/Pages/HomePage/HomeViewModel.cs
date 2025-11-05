using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using BiliLive.Core.Interface;
using BiliLive.Core.Models.BiliService;
using BiliLive.Resources;
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

//WeakReferenced传递刷新直播数据命令
public class StartRefreshLiveInfoMessage(string streamUrl, string streamKey , string liveKey)
{
    public string StreamUrl { get; } = streamUrl;
    public string StreamKey { get; } = streamKey;
    public string LiveKey { get; } = liveKey;
}
public class StopRefreshLiveInfoMessage;


public partial class HomeViewModel : ViewModelBase
{ 
    public const string LiveUrlFormat = "https://live.bilibili.com";
    private const string EmptyText = "未获取";

    public static GeneralState GeneralState => General.State;
    
    [ObservableProperty] private Bitmap? _userFace;
    
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(IsRoomTitleChanged))]
    private string? _inputRoomTitle;

    private string? _roomTitle;
    public bool IsRoomTitleChanged => GeneralState.UserId != null && !string.IsNullOrWhiteSpace(InputRoomTitle)  && _roomTitle != InputRoomTitle;

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
        
        WeakReferenceMessenger.Default.Register<StartRefreshLiveInfoMessage>(this,  (o, m) =>
        {
            _ = Task.Run(async () => await UpdateLiveInfoAsync(m.StreamUrl, m.StreamKey, m.LiveKey));
        });
        WeakReferenceMessenger.Default.Register<StopRefreshLiveInfoMessage>(this,  (_, _) =>
        {
            _liveDataCts.Cancel();
        });
        
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
        
        General.State.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(General.State.UserFaceByte):
                    RefreshUserFaceAsync(General.State.UserFaceByte);
                    break;
                case nameof(General.State.UserId):
                    OnPropertyChanged(nameof(IsRoomTitleChanged));
                    if (GeneralState.UserId == null)
                    {
                        InputRoomTitle = null;
                        using var coverStream = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/defaultCover.jpg"));
                        _roomCover?.Dispose();
                        _roomCover = PicHelper.ResizeStreamToBitmap(coverStream, 132 * 2, 74 * 2);
                    }
                    break;
            }
        };
    }

    private void RefreshUserFaceAsync(byte[]? userFaceByte)
    {
        try
        {
            //刷新用户头像
            if (userFaceByte == null || userFaceByte.Length == 0) { return; }
            using var ms = new MemoryStream(userFaceByte);
            UserFace?.Dispose();
            UserFace = PicHelper.ResizeStreamToBitmap(ms,116,116);
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public async Task LoadHomeVmAsync(LoginResult loginResult)
    {
        if (loginResult is LoginSuccess result)
        {
            //获取用户信息
            GeneralState.UserFaceByte = result.UserFaceBytes;
            using var ms = new MemoryStream(result.UserFaceBytes);
            UserFace = PicHelper.ResizeStreamToBitmap(ms, 71 * 2, 71 * 2) ?? new Bitmap(ms);

            //获取直播间信息
            var roomInfo = await _biliService.GetRoomInfoAsync();
            GeneralState.RoomId = roomInfo.RoomId;
            GeneralState.UserId = result.UserId;
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
        if (!General.State.IsLogin) { return; }
        AreaSelectorVm.IsLogged = true;
        AreaSelectorVm.RefreshAreasCommand.Execute(null);
        if (AreaSelectorVm.BiliService != null) return;
        AreaSelectorVm.BiliService = _biliService;
        AreaSelectorVm.LoadAreasCommand.Execute(null);
    }
    
    [RelayCommand]
    private async Task OpenRoomUrlAsync()
    {
        if (GeneralState.RoomId == null) return;
        try
        { 
            // BrowserUtil.OpenInBrowser($"{LiveUrlFormat}/{GeneralState.RoomId}");
            await AvaloniaUtils.OpenUrl($"{LiveUrlFormat}/{GeneralState.RoomId}");
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
                CopyTarget.RoomUrl => GeneralState.RoomId == null ? string.Empty : $"{LiveUrlFormat}/{GeneralState.RoomId}",
                CopyTarget.StreamKey => StreamKey,
                CopyTarget.StreamUrl => ApiUrl,
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            };
            if (string.IsNullOrEmpty(text) || text.Equals(EmptyText)) return;
            // var clipboard = ClipboardHelper.Get();
            var clipboard = AvaloniaUtils.GetClipboard();
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
    
    [RelayCommand]
    private async Task ChangeRoomCoverAsync()
    {
        var filePath = await AvaloniaUtils.PickFileAsync("选择要上传的直播间封面",[".jpg",".jpeg",".png"]);
        if (filePath == null) return;
        try
        {
            var fileBytes = await File.ReadAllBytesAsync(filePath);
            await _biliService.ChangeRoomCoverAsync(fileBytes);
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("更换封面成功",Geometry.Parse(MdIcons.Check)));
            var roomInfo = await _biliService.GetRoomInfoAsync();
            using var rcMs = new MemoryStream(roomInfo.RoomCover);
            RoomCover?.Dispose();
            RoomCover = PicHelper.ResizeStreamToBitmap(rcMs, 132 * 2, 74 * 2) ?? new Bitmap(rcMs);
        }
        catch (Exception ex)
        {
            await ShowWindowHelper.ShowErrorAsync("更换封面失败。\n错误信息：" + ex.Message);
        }
    }
    
    //更新直播间信息任务任务
    private CancellationTokenSource _liveDataCts = new();
    private bool _isUpdatingLiveInfo;
    private async Task UpdateLiveInfoAsync(string streamUrl, string streamKey, string liveKey)
    {
        if (_isUpdatingLiveInfo) { throw new InvalidOperationException("直播数据更新任务已经在运行中，不能重复启动"); }
        _isUpdatingLiveInfo = true;
        
        StreamKey = streamKey;
        ApiUrl = streamUrl;

        var token = _liveDataCts.Token;
        var errorRetryTime = 0;

        try
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(5000, token);
                await UpdateLiveInfoTask();
            }
        }
        catch (OperationCanceledException)
        {
            //最后一次获取直播数据
            await UpdateLiveInfoTask();
        }
        finally
        {
            _liveDataCts.Dispose();
            _liveDataCts = new CancellationTokenSource();
            _isUpdatingLiveInfo = false;
        }
        
        return;
        
        async Task UpdateLiveInfoTask()
        {
            try
            {
                var liveData = await _biliService.GetLiveDataAsync(liveKey);
                // Console.WriteLine(liveData);
                // 结构 {"code":0,"message":"0","ttl":1,"data":{"LiveTime":543,"AddFans":0,"HamsterRmb":0,"NewFansClub":0,"DanmuNum":0,"MaxOnline":2,"WatchedCount":1}}
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
                errorRetryTime = 0;
            }
            catch (Exception ex)
            {
                errorRetryTime += 1;
                if (errorRetryTime > 10)
                {
                    await Dispatcher.UIThread.InvokeAsync( async () => { await ShowWindowHelper.ShowErrorAsync($"获取直播间数据失败超过10次，出错数据:{ex.Message}"); });
                    _isUpdatingLiveInfo = false;
                    await _liveDataCts.CancelAsync();
                }
            }
        }
    }
}