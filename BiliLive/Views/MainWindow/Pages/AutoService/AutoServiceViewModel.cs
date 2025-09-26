using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using BiliLive.Core.Models.BiliService;
using BiliLive.Core.Services;
using BiliLive.Models;
using BiliLive.Resources;
using BiliLive.Services;
using BiliLive.Utils;
using BiliLive.Views.DialogWindow;
using BiliLive.Views.MainWindow.Pages.AutoService.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BiliLive.Views.MainWindow.Pages.AutoService;

public partial class AutoServiceViewModel : ViewModelBase
{
    [ObservableProperty]private ObservableCollection<Alt> _altsList = [];
    
    [ObservableProperty] private string? _ffmpegPath;
    [ObservableProperty] private string? _videoPath;
    
    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private bool _isAutoStart;
    [ObservableProperty] private bool _isRandomSecond;
    [ObservableProperty] private bool _isCheck60MinTask;
    
    //时间
    [ObservableProperty] private string? _startHour;
    [ObservableProperty] private string? _startMinute;
    [ObservableProperty] private string? _startSecond;
    
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedStartTime))]
    private TimeSpan _startTime = new(0, 5, 0);
    public string FormattedStartTime => StartTime.ToString(@"hh\:mm\:ss");
    
    
    //初始化数据
    [RelayCommand]
    private async Task InitializeAsync()
    {
        var config = await ConfigManager.LoadConfigAsync();
        if (config is null) return;
        if (config.Alts?.Length > 0)
        {
            foreach (var altSettings in config.Alts)
            {
                if (altSettings != null) AltsList.Add(await Alt.CreateAltAsync(altSettings, RemoveAlt));
            }
        }
    }
    
    [RelayCommand]
    private async Task ToggleOptions()
    {
        var random = new Random();
        // var hour   = int.TryParse(StartHour,   out var h) ? h : 0;
        // var minute = int.TryParse(StartMinute, out var m) ? m : 0;
        // var second = int.TryParse(StartSecond, out var s) ? s : 0;
        //
        // var seconds = hour * 3600 +
        //               minute * 60 +
        //               (IsRandomSecond ? random.Next(-240, -120) : second);
        
        
        //设置第二天基准
  
        var baseSeconds = StartTime.TotalSeconds;
        var finalSeconds = IsRandomSecond ? baseSeconds + random.Next(-240, -120) : baseSeconds;
        var streamTime = DateTime.Today.AddDays(1).AddSeconds(finalSeconds);
        
        // WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"设置时间{streamTime}",Geometry.Parse(MdIcons.Check)));
        
        
        var popupMsg = IsEnabled ? $"自动开播将在 {streamTime} 开始" : "已关闭自动开播服务";
        var icon = IsEnabled ? Geometry.Parse(MdIcons.Check) : Geometry.Parse(MdIcons.Error);
        WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(popupMsg,icon));
        await ConfigManager.SaveConfigAsync(ConfigType.EnableAutoService,IsEnabled);
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
    private async Task ToggleAutoStartOptionAsync()
    {
        await ConfigManager.SaveConfigAsync(ConfigType.AutoStart,IsAutoStart);
    }
    
    [RelayCommand]
    private async Task ToggleCheck60MinTaskAsync()
    {
        await ConfigManager.SaveConfigAsync(ConfigType.Check60MinTask,IsCheck60MinTask);
    }
    

    [RelayCommand]
    private async Task AddAltsAsync()
    {
        //弹出添加账号窗口
        using var altVm = new AltsManagerViewModel(false);
        await ShowWindowHelper.ShowWindowAsync(new AltsManager(){DataContext = altVm});
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
                // DanmakuList = altVm.DanmakuList,
                ProxyAddress = altVm.ProxyAddress,
                ProxyUsername = altVm.ProxyUsername,
                ProxyPassword = altVm.ProxyPassword
            };
            
            AltsList.Add(await Alt.CreateAltAsync(altSettings, RemoveAlt));
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"登录成功，当前账号 {altSettings.UserName}",Geometry.Parse(MdIcons.Check)));
        }
    }
    
    private void RemoveAlt(Alt alt)
    {
        if (AltsList.Contains(alt))
            AltsList.Remove(alt);
    }
}

public partial class Alt : ObservableObject
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
    private readonly AltSettings _altSettings;
    
    private Alt(AltSettings altSettings, Action<Alt> removeCallback)
    {
        //赋值传入数据
        _altService = new AltService(altSettings.CookieString,
            altSettings.ProxyAddress,
            altSettings.ProxyUsername,
            altSettings.ProxyPassword);
        
        _altSettings = altSettings;
        _removeCallback = removeCallback;
        UserName = altSettings.UserName;
        IsGiftSent = altSettings.IsSendGift;
    }
    
    //工厂方法构造函数
    public static async Task<Alt> CreateAltAsync(AltSettings altSettings, Action<Alt> removeCallback)
    {
        var alt = new Alt(altSettings, removeCallback);
        await alt.InitializeAsync();
        return alt;
    }
    
    //初始化账号信息
    private async Task InitializeAsync()
    {
        var loginResult= await _altService.LoginAsync(_altSettings.CookieString);
        if (loginResult is LoginSuccess result)
        {
            UserId = result.UserId;
            UserName = result.UserName;
            using var ms = new MemoryStream(result.UserFaceBytes);
            UserFace = await Task.Run(() => Bitmap.DecodeToWidth(ms, 80));
            //保存账号到本地
            await SaveAltSettingsAsync();
        }
    }
    
    [RelayCommand]
    private async Task AltSettingsAsync()
    {
        using var altVm = new AltsManagerViewModel(true);
        altVm.CookieValue = _altSettings.CookieString;
        altVm.AllowDoneClose = true;

        await ShowWindowHelper.ShowWindowAsync(new AltsManager(){DataContext = altVm});
        if (altVm.AllowDoneClose)
        {
            //更新设置
            _altSettings.CookieString = altVm.CookieValue;
                
            await InitializeAsync();
                
            _altSettings.IsSendGift = altVm.IsSendGift;
            _altSettings.ProxyAddress = altVm.ProxyAddress;
            _altSettings.ProxyUsername = altVm.ProxyUsername;
            _altSettings.ProxyPassword = altVm.ProxyPassword;

            await SaveAltSettingsAsync();

        }
    }
    
    [RelayCommand]
    private async Task SendDanmakuAsync()
    {
        var dialogVm = new DialogWindowViewModel
        {
            Message = $"确认发送弹幕吗？",
        };
        await ShowWindowHelper.ShowWindowAsync(new DialogWindow.DialogWindow(){DataContext =dialogVm});
        if (dialogVm.IsConfirmed)
        {
            //待办 发送弹幕
            await _altService.SendDanmakuAsync("您好，这是一条测试弹幕");
            IsDanmakuSent = true;
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"已为账号 {UserName} 发送弹幕",Geometry.Parse(MdIcons.Check)));
        }
    }

    [RelayCommand]
    private async Task SendGiftAsync()
    {
        var dialogVm = new DialogWindowViewModel
        {
            Message = $"确认发送礼物吗？",
        };
        await ShowWindowHelper.ShowWindowAsync(new DialogWindow.DialogWindow(){DataContext =dialogVm});
        if (dialogVm.IsConfirmed)
        {
            //待办 发送礼物
            IsGiftSent = true;
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"已为账号 {UserName} 发送礼物",Geometry.Parse(MdIcons.Check)));
        }
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
                await ConfigManager.RemoveAltSettingsAsync(_altSettings);
                
                //删除账号时才释放服务
                _altService.Dispose();
                UserFace?.Dispose();
                WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("已清除当前账号",Geometry.Parse(MdIcons.Check)));
            }
       
    }
    
    //保存账号
    private async Task SaveAltSettingsAsync()
    {
        await ConfigManager.SaveAltSettingsAsync(_altSettings);
    }
}