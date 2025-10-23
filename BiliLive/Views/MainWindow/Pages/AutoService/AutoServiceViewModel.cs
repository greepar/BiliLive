using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using BiliLive.Core.Interface;
using BiliLive.Core.Models.BiliService;
using BiliLive.Core.Services;
using BiliLive.Core.Services.BiliService;
using BiliLive.Models;
using BiliLive.Resources;
using BiliLive.Utils;
using BiliLive.Views.DialogWindow;
using BiliLive.Views.MainWindow.Pages.AutoService.Components;
using BiliLive.Views.MainWindow.Pages.HomePage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLive.Views.MainWindow.Pages.AutoService;

public enum ToggleOption
{
    //总服务
    AutoStreamService,
      //AutoStream自服务
      AutoStart,
      RandomSecond,
      Check60MinTask,
      
    AutoGiftService,
    AutoClaimRewardService,
   
}
public partial class AutoServiceViewModel : ViewModelBase
{
    public AppConfig? Config;
    [ObservableProperty] private ObservableCollection<Alt> _altsList = [];
    public bool HasAlts => AltsList.Count > 0;
    
    [ObservableProperty] 
    private string? _ffmpegPath;
    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsCoreSet))]
    [NotifyPropertyChangedFor(nameof(CoreStatus))]
    private string? _videoPath;
    public bool IsCoreSet => !string.IsNullOrWhiteSpace(FfmpegPath) && !string.IsNullOrWhiteSpace(VideoPath);
    public string CoreStatus => IsCoreSet ? "已配置" : "未配置";
    
    [ObservableProperty] private bool? _isInAutoStreaming = false;
    [ObservableProperty] private string _autoStreamingStatusText = "未启动";
    
    [ObservableProperty] private bool _isAutoStreamEnabled;
    [ObservableProperty] private bool _isAutoStart;
    [ObservableProperty] private bool _isRandomSecond;
    [ObservableProperty] private bool _isCheck60MinTask;

    [ObservableProperty] 
    private bool _isChecked;
    
    [ObservableProperty] private bool _isAutoClaimRewardEnabled;
    
    
    //小号相关
    [ObservableProperty] private bool _isAltGiftServiceEnabled;
    
    //时间
    [ObservableProperty] private string? _startHour;
    [ObservableProperty] private string? _startMinute;
    [ObservableProperty] private string? _startSecond;

    private readonly IBiliService _biliService;
    public AutoServiceViewModel(IServiceProvider? serviceProvider = null)
    {
        if (!Design.IsDesignMode && serviceProvider != null)
        {
            _biliService = serviceProvider.GetService<IBiliService>() ?? new BiliServiceImpl();
        }
        else
        {
            _biliService = new BiliServiceImpl();
        }
        //订阅AltsList变化 -> 更新HasAlts
        AltsList.CollectionChanged += (_, _) => OnPropertyChanged(nameof(HasAlts)); 
    }
    
    
    //初始化数据
    [RelayCommand]
    private async Task InitializeAsync()
    {
        if (IsAutoStart)
        {
            if (!IsCoreSet)
            {
                WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("请先配置直播核心",Geometry.Parse(MdIcons.Error)));
                IsAutoStreamEnabled = false;
                return;
            }
            IsAutoStreamEnabled = true;
            _ = Task.Run(async () => await SetAutoStreamAsync());
        }
        if ( Config is { Alts.Count: > 0 })
        {
            //并行创建Alt实例
            var tasks = Config.Alts
                .Where(_ => true)
                .Select(async altSettings =>
                {
                    try
                    {
                        var alt = await Alt.CreateAltAsync(altSettings,_biliService, RemoveAlt);
                        return new { Result = (Alt?)alt , AltSettings = altSettings , ExceptionMsg = (string?)null };
                    }
                    catch (Exception ex)
                    {
                        return new { Result = (Alt?)null, AltSettings = altSettings , ExceptionMsg = (string?)ex.Message };
                    }
                });
            var results = await Task.WhenAll(tasks);
            
            // 分别处理成功和失败的结果
            foreach (var res in results)
            {
                if (res.Result != null)
                {
                    AltsList.Add(res.Result);
                }
                if (res.ExceptionMsg != null)
                {
                    await ShowWindowHelper.ShowErrorAsync(
                        $"账号 {res.AltSettings.UserName} 添加失败，可能是Cookie无效或网络异常，请重新添加。\n错误信息：{res.ExceptionMsg}");
                }
            }
        }
    }
    
    [RelayCommand]
    public async Task ToggleOptions(ToggleOption option)
    {
        //根据不同选项开关执行不同操作
        switch (option)
        {
            case ToggleOption.AutoStart:
                await ConfigManager.SaveConfigAsync(ConfigType.AutoStart,IsAutoStart);
                break;
            case ToggleOption.AutoStreamService:
                if (!IsCoreSet)
                {
                    WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("请先配置直播核心",Geometry.Parse(MdIcons.Error)));
                    IsAutoStreamEnabled = false;
                    return;
                }
                if (!IsAutoStreamEnabled)
                {
                    // IsAltGiftServiceEnabled = false;
                    // IsAutoClaimRewardEnabled = false;
                    if (_autoStreamCts != null) await _autoStreamCts.CancelAsync();
                    return;
                }
                _ = Task.Run(async () => await SetAutoStreamAsync());
                break;
            case ToggleOption.Check60MinTask:
                await ConfigManager.SaveConfigAsync(ConfigType.Check60MinTask,IsCheck60MinTask);
                break;
            case ToggleOption.RandomSecond:
                // await ConfigManager.SaveConfigAsync(ConfigType.,IsRandomSecond);
                break;
            case ToggleOption.AutoGiftService:
                if (!IsAutoStreamEnabled)
                {
                    // WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("请先开启自动直播任务",Geometry.Parse(MdIcons.Error)));
                    // IsAltGiftServiceEnabled = false;
                }
                break;
            case ToggleOption.AutoClaimRewardService:
                if (!IsAutoStreamEnabled)
                {
                    // WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("请先开启自动直播任务",Geometry.Parse(MdIcons.Error)));
                    // IsAutoClaimRewardEnabled = false;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(option), option, null);
        }
    }

    //设置定时Cts
    private CancellationTokenSource? _autoStreamCts;
    private async Task SetAutoStreamAsync()
    {
        if (_autoStreamCts?.IsCancellationRequested == true) { _autoStreamCts.Dispose(); _autoStreamCts = null; }
        _autoStreamCts ??= new CancellationTokenSource(); 
        var token = _autoStreamCts.Token;
        var isStreamStartedByThis = false;
        try
        {
            //等待开始时间
            var random = new Random();
            var hour = int.TryParse(StartHour, out var h) ? h : 0;
            var minute = int.TryParse(StartMinute, out var m) ? m : 0;
            var second = int.TryParse(StartSecond, out var s) ? s : 0;

            var seconds = hour * 3600 +
                          minute * 60 +
                          (IsRandomSecond ? random.Next(-240, -120) : second);
            var startTime = TimeSpan.FromSeconds(seconds);

            //设置第二天基准
            var baseSeconds = startTime.TotalSeconds;
            var finalSeconds = IsRandomSecond ? baseSeconds + random.Next(-240, -120) : baseSeconds;
            var streamTime = DateTime.Today.AddDays(1).AddSeconds(finalSeconds);

            //等待直到开始时间
            if (streamTime < DateTime.Now)
            {
                //如果时间已过则推迟一天
                var originalTime = streamTime;
                Dispatcher.UIThread.Post(() =>
                {
                    WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(
                        $"今天直播时间点已无法到达{originalTime - DateTime.Now}时间后开始", Geometry.Parse(MdIcons.Check)));
                });
                streamTime = streamTime.AddDays(1);
            }

            Dispatcher.UIThread.Post(() =>
            {
                WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"自动开播将在 {streamTime} 开始",
                    Geometry.Parse(MdIcons.Check)));
            });
            IsInAutoStreaming = null;
            AutoStreamingStatusText = $"等待自动开播，开始时间：{streamTime}";
            // await Task.Delay(streamTime - DateTime.Now, token); 
            AutoStreamingStatusText = $"正在自动直播中...";

            //先检查直播是否被占用
            if (_biliService.IsStreaming)
            {
                const string errText = "当前正在直播,跳过执行今天自动任务。";
                Dispatcher.UIThread.Post(() =>
                {
                    WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(errText,
                        Geometry.Parse(MdIcons.Check)));
                });
                IsInAutoStreaming = false;
                AutoStreamingStatusText = errText;
                //推迟一天直播
                //TODO:推迟一天直播的相关提示
                await Task.Delay(DateTime.Now.AddDays(1) - DateTime.Now, token);
            }

            if (IsCheck60MinTask)
            {
                //TODO:检查60分钟任务

                // var hasTask = await _biliService.Check60MinTaskAsync();
                // if (!hasTask)
                // {
                //     Dispatcher.UIThread.Post(() => { WeakReferenceMessenger.Default.Send(new ShowNotificationMessage( $"未检测到60分钟任务，自动开播任务取消",Geometry.Parse(MdIcons.Error))); });
                //     IsAutoStreamEnabled = false;
                //     await ConfigManager.SaveConfigAsync(ConfigType.EnableAutoService,IsAutoStreamEnabled);
                //     return;
                // }
            }

            //开启直播接口
            var startLiveResponse = await _biliService.StartLiveAsync();
            var streamKey = startLiveResponse.GetProperty("data").GetProperty("rtmp").GetProperty("code").GetString();
            var streamUrl = startLiveResponse.GetProperty("data").GetProperty("rtmp").GetProperty("addr").GetString();
            var liveKey = startLiveResponse.GetProperty("data").GetProperty("live_key").GetString();
            if (FfmpegPath == null || VideoPath == null || streamUrl == null || streamKey == null || liveKey == null)
            {
                throw new Exception("获取推流地址失败");
            }

            //启动发送小号礼物和弹幕线程
            _ = Task.Run(async () =>
            {
                try
                {
                    //随机等待20-40分钟
                    await Task.Delay(random.Next(1200 * 1000, 2400 * 1000), token);
                    if (IsAltGiftServiceEnabled && HasAlts)
                    {
                        foreach (var alt in AltsList)
                        {
                            if (alt is { IsGiftSent: true, IsDanmakuSent: true }) continue;
                            if (alt.AltSettings.IsSendGift)
                            {
                                await alt.SendGiftAsync(false);
                            }

                            await alt.SendDanmakuAsync(false);
                            await Task.Delay(2000, token);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Console.WriteLine("小号礼物和弹幕发送任务取消");
                    //任务取消
                }
            }, token);

            //启用Ffmpeg线程直播大约一小时
            var liveDurationSec = IsRandomSecond ? random.Next(3600, 3800) : 3600;
            WeakReferenceMessenger.Default.Send(new StartRefreshLiveInfoMessage(streamUrl, streamKey, liveKey));
            _ = Task.Run(async () =>
            {
                //TODO:失败后重试机制
                //随机直播大于1个小时防止被检测

                try
                {
                    await FfmpegWrapper.StartStreamingAsync(FfmpegPath, VideoPath, streamUrl, streamKey,
                        liveDurationSec);
                }
                catch (Exception ex)
                {
                    Dispatcher.UIThread.Post(async void () =>
                    {
                        try
                        {
                            IsAutoStreamEnabled = false;
                            IsInAutoStreaming = false;
                            await _autoStreamCts.CancelAsync();
                            await ShowWindowHelper.ShowErrorAsync("自动开播任务出现错误。\n错误信息：" + ex.Message);
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    });
                }
            }, token);

            //设置biliService直播状态
            _biliService.IsStreaming = true;
            isStreamStartedByThis = true;
            
            await Task.Delay(liveDurationSec * 1000, token);
            IsInAutoStreaming = true;
            AutoStreamingStatusText = "今日直播已完成";
        }
        catch (OperationCanceledException)
        {
            //通知UI线程
            Dispatcher.UIThread.Post(() =>
            {
                WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"自动开播任务已取消",
                    Geometry.Parse(MdIcons.Check)));
            });
            IsInAutoStreaming = false;
            AutoStreamingStatusText = "自动开播任务未开启";
            await FfmpegWrapper.InterruptStreamingAsync();
        }
        catch (Exception ex)
        {
            Dispatcher.UIThread.Post(async void () =>
            {
                try
                {
                    IsAutoStreamEnabled = false;
                    IsInAutoStreaming = false;
                    AutoStreamingStatusText = "自动开播异常中止";
                    await ShowWindowHelper.ShowErrorAsync("自动开播任务出现错误。\n错误信息：" + ex.Message);
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }
        finally
        {
            if (isStreamStartedByThis)
            {
                _biliService.IsStreaming = false;
                WeakReferenceMessenger.Default.Send(new StopRefreshLiveInfoMessage());
            }
        }
    }
    
    [RelayCommand]
    private async Task PickFfmpegPathAsync()
    {
        string[] defaultFileExtension = OperatingSystem.IsWindows() ? [".exe"] : [""];
        var pickFile = await FolderPickHelper.PickFileAsync("Choose Ffmpeg Path",defaultFileExtension);
        if (string.IsNullOrWhiteSpace(pickFile))
            return;
        if (await FfmpegWrapper.CheckFfmpegAvailableAsync(pickFile))
        {
            FfmpegPath = pickFile;
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("Ffmpeg路径有效",Geometry.Parse(MdIcons.Check)));
            await ConfigManager.SaveConfigAsync(ConfigType.FfmpegPath,FfmpegPath);
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("Ffmpeg路径无效，请重新选择",Geometry.Parse(MdIcons.Error)));
        }
    }
    
    [RelayCommand]
    private async Task PickVideoPathAsync()
    {
        if (string.IsNullOrWhiteSpace(FfmpegPath))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("请先设置Ffmpeg路径",Geometry.Parse(MdIcons.Notice)));
            return;
        }
        var pickFile = await FolderPickHelper.PickFileAsync("Choose Video Path",[".mp4",".flv",".mkv",".mov",".avi"]);
        if (string.IsNullOrWhiteSpace(pickFile))
            return;
        if (!await FfmpegWrapper.CheckVideoAvailableAsync(FfmpegPath,pickFile))
        { 
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("视频文件无效，请重新选择",Geometry.Parse(MdIcons.Error)));
            return;
        }
     
        VideoPath = pickFile;
        WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("视频文件有效",Geometry.Parse(MdIcons.Check)));
        await ConfigManager.SaveConfigAsync(ConfigType.VideoPath,VideoPath);
    }

    [RelayCommand]
    private async Task AddAltsAsync()
    {
        //弹出添加账号窗口
        using var altVm = new AltManagerViewModel(false);
        await ShowWindowHelper.ShowWindowAsync(new AltManager(){DataContext = altVm});
        if (altVm is { AllowDoneClose: true, CookieValue: {Length: > 0} cookie ,UserName: { Length: > 0 } userName})
        {
            //防止重复添加
            foreach (var alt in AltsList)
            {
                if (alt.UserName == userName)
                {
                    WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"账号 {userName} 已存在，请勿重复添加",Geometry.Parse(MdIcons.Error)));
                    return;
                }
            }

            var altSettings = new AltSettings()
            {
                CookieString = cookie,
                UserName = userName,
                IsSendGift = altVm.IsSendGift,
                ProxyInfo = altVm.ProxyAddress == null ? null : new ProxyInfo
                {
                    ProxyAddress = altVm.ProxyAddress,
                    Username = altVm.ProxyUsername,
                    Password = altVm.ProxyPassword
                }
            };
            
            AltsList.Add(await Alt.CreateAltAsync(altSettings,_biliService,RemoveAlt));
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"登录成功，当前账号 {altSettings.UserName}",Geometry.Parse(MdIcons.Check)));
        }
    }
    
    [RelayCommand]
    private async Task ClaimAwardAsync(string taskId)
    {
        if (string.IsNullOrWhiteSpace(taskId))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"请输入任务ID",Geometry.Parse(MdIcons.Check)));
            return;
        }
        await _biliService.ClaimAwardAsync(taskId);
    }
    
    private void RemoveAlt(Alt alt)
    {
        if (AltsList.Contains(alt))
            AltsList.Remove(alt);
    }
}


//小号类
public partial class Alt : ObservableObject , IDisposable
{
    //先赋值
    [ObservableProperty] private string _userName;
    [ObservableProperty] private bool _isGiftSent;
    [ObservableProperty] private bool _isDanmakuSent;
 
    //后获取信息
    [ObservableProperty] private Bitmap? _userFace;
    [ObservableProperty] private long? _userId;
   
   //移除账号回调 
    private readonly Action<Alt> _removeCallback;
    
    //公共服务
    private readonly AltService _altService;
    public readonly AltSettings AltSettings;
    
    private Alt(AltSettings altSettings,IBiliService biliService,Action<Alt> removeCallback)
    {
        //赋值传入数据
        _altService = new AltService(biliService,altSettings.CookieString, altSettings.ProxyInfo);
        AltSettings = altSettings;
        _removeCallback = removeCallback;
        UserName = altSettings.UserName;
        IsGiftSent = altSettings.IsSendGift;
    }
    
    //工厂方法构造函数
    public static async Task<Alt> CreateAltAsync(AltSettings altSettings, IBiliService biliService ,Action<Alt> removeCallback)
    {
        var alt = new Alt(altSettings, biliService ,removeCallback);
        await alt.InitializeAsync();
        return alt;
    }
    
    //初始化账号信息
    private async Task InitializeAsync()
    {
        var loginResult= await _altService.LoginAsync(AltSettings.CookieString);
        switch (loginResult)
        {
            case LoginSuccess result:
            {
                UserId = result.UserId;
                UserName = result.UserName;
                using var ms = new MemoryStream(result.UserFaceBytes);
                UserFace = PicHelper.ResizeStreamToBitmap(ms, 80, 80);
                //保存账号到本地
                await ConfigManager.SaveAltSettingsAsync(AltSettings);
                break;
            }
            case LoginFailed failed:
            {
                UserId = 0;
                await ShowWindowHelper.ShowErrorAsync($"小号 [ {AltSettings.UserName} ] 登录错误:{failed.ErrorMsg}");
                break;
            }
        }
    }
    
    [RelayCommand]
    private async Task AltSettingsAsync()
    {
        using var altVm = new AltManagerViewModel(true);
        //将配置文件的值载入altVM
        altVm.ProxyAddress = AltSettings.ProxyInfo?.ProxyAddress;
        altVm.ProxyUsername = AltSettings.ProxyInfo?.Username;
        altVm.ProxyPassword = AltSettings.ProxyInfo?.Password;
        altVm.CookieValue = AltSettings.CookieString;
        altVm.IsSendGift = AltSettings.IsSendGift;
        altVm.AllowDoneClose = true;
        if (AltSettings.DanmakuList != null)
        {
            foreach (var danmaku in AltSettings.DanmakuList)
            {
                altVm.AddDanmaku(danmaku);
            }
        }
        //弹出窗口
        await ShowWindowHelper.ShowWindowAsync(new AltManager {DataContext = altVm});
        //确认后将VM的值写入配置文件
        if (altVm.AllowDoneClose)
        {
            AltSettings.CookieString = altVm.CookieValue;
            AltSettings.IsSendGift = altVm.IsSendGift;
            AltSettings.DanmakuList = altVm.DanmakuList
                .Where(x => !string.IsNullOrWhiteSpace(x.Text))
                .Select(x => x.Text)
                .ToArray();
            var vmProxyInfo = altVm.ProxyAddress == null ? null : new ProxyInfo
            {
                ProxyAddress = altVm.ProxyAddress,
                Username = altVm.ProxyUsername,
                Password = altVm.ProxyPassword
            };
            if (vmProxyInfo?.ProxyAddress != AltSettings.ProxyInfo?.ProxyAddress)
            {
                try
                {
                    if (vmProxyInfo != null) await _altService.TryAddNewProxy(vmProxyInfo);
                    await InitializeAsync();
                }
                catch (Exception ex)
                {
                    await ShowWindowHelper.ShowErrorAsync("代理无法使用，请检查代理地址和端口。\n错误信息：" + ex.Message);
                    return;
                }
            }
            AltSettings.ProxyInfo = vmProxyInfo;
            await ConfigManager.SaveAltSettingsAsync(AltSettings);
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"已更新账号 {UserName} 的配置",Geometry.Parse(MdIcons.Check)));
        }
    }
    
    [RelayCommand]
    public async Task SendDanmakuAsync(bool isManual)
    {
        if (isManual)
        {
            var dialogVm = new DialogWindowViewModel
            {
                Message = $"确认重新发送弹幕吗？",
            };
            await ShowWindowHelper.ShowWindowAsync(new DialogWindow.DialogWindow(){DataContext =dialogVm});
            if (!dialogVm.IsConfirmed)
            {
                return;
            }
        }
        
        if (AltSettings.DanmakuList == null || AltSettings.DanmakuList.Length == 0)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"账号 {UserName} 未设置弹幕内容，请先设置",Geometry.Parse(MdIcons.Error)));
            return;
        }
        
        try
        {
            foreach (var danmaku in AltSettings.DanmakuList)
            {
                await _altService.SendDanmakuAsync(danmaku);
                var delay = new Random().Next(1000, 12000);
                await Task.Delay(delay);
            }
        }
        catch (Exception ex)
        {
            await ShowWindowHelper.ShowErrorAsync("发送弹幕失败，请检查网络或者代理设置。\n错误信息：" + ex.Message);
            return;
        }
        
        IsDanmakuSent = true;
        Dispatcher.UIThread.Post(() => { WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"已用账号 {UserName} 发送弹幕",Geometry.Parse(MdIcons.Check))); });
    }

    [RelayCommand]
    public async Task SendGiftAsync(bool isManual)
    {
        if (isManual)
        {
            var dialogVm = new DialogWindowViewModel
            {
                Message = $"确认重新发送礼物吗？",
            };
            await ShowWindowHelper.ShowWindowAsync(new DialogWindow.DialogWindow(){DataContext =dialogVm});
            if (!dialogVm.IsConfirmed)
            {
                return;
            }
        }
        
        try
        {
            await _altService.SendGiftAsync();
        }
        catch (Exception ex)
        {
            await ShowWindowHelper.ShowErrorAsync("发送弹幕失败，请检查网络或者代理设置。\n错误信息：" + ex.Message);
            return;
        }
        
        IsGiftSent = true;
        Dispatcher.UIThread.Post(() => { WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"已为账号 {UserName} 发送礼物",Geometry.Parse(MdIcons.Check))); });
    }
    
    [RelayCommand]
    private async Task RemoveAltAsync()
    {
            //清除当前账号
            var dialogVm = new DialogWindowViewModel
            {
                Message = $"确认清除账号 {UserName} 吗？",
            };
            await ShowWindowHelper.ShowWindowAsync(new DialogWindow.DialogWindow(){DataContext =dialogVm});
            if (dialogVm.IsConfirmed)
            {
                _removeCallback(this);
                await ConfigManager.RemoveAltSettingsAsync(AltSettings);
                
                //删除账号时才释放服务
                Dispose();
                Dispatcher.UIThread.Post(() => { WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("已清除当前账号",Geometry.Parse(MdIcons.Check))); });
            }
       
    }
    
    //释放资源
    public void Dispose()
    {
        _altService.Dispose();
        UserFace?.Dispose();
        GC.SuppressFinalize(this);
    }
}