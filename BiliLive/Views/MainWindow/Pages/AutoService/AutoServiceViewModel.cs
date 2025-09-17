using System;
using System.Threading.Tasks;
using Avalonia.Media;
using BiliLive.Core.Services;
using BiliLive.Models;
using BiliLive.Resources;
using BiliLive.Services;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BiliLive.Views.MainWindow.Pages.AutoService;

public partial class AutoServiceViewModel : ViewModelBase
{
    [ObservableProperty] private string? _ffmpegPath;
    [ObservableProperty] private string? _videoPath;
    
    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private bool _isAutoStart;
    [ObservableProperty] private bool _isRandomSecond;
    [ObservableProperty] private bool _isCheck60MinTask;
    
    //时间
    [ObservableProperty] private int? _startHour;
    [ObservableProperty] private int? _startMinute;
    [ObservableProperty] private int? _startSecond;
    
    [RelayCommand]
    private async Task ToggleOptions()
    {
        var random = new Random();
        var seconds = (StartHour ?? 0) * 3600 +
                      (StartMinute ?? 0) * 60.0 + 
                      (IsRandomSecond ? random.Next(-240,-120) : (StartSecond ?? 0));

        if (seconds <= 0)
        {
            return;
        }
        
        //设置第二天基准
        var streamTime = DateTime.Today.AddDays(1).AddSeconds(seconds);

       
        WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"设置时间{streamTime}",Geometry.Parse(MdIcons.Error)));
        
          
       
        
        var popupMsg = IsEnabled ? "已启用自动开播服务" : "已关闭自动开播服务";
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
    private async Task AutoStartOptionAsync()
    {
        await ConfigManager.SaveConfigAsync(ConfigType.AutoStart,IsAutoStart);
    }
    
    [RelayCommand]
    private async Task Check60MinTaskAsync()
    {
        await ConfigManager.SaveConfigAsync(ConfigType.Check60MinTask,IsCheck60MinTask);
    }
    
    
    
}