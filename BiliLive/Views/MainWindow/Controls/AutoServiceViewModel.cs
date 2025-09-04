using System;
using System.Diagnostics;
using System.Threading.Tasks;
using BiliLive.Core.Services;
using BiliLive.Models;
using BiliLive.Services;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BiliLive.Views.MainWindow.Controls;

public partial class AutoServiceViewModel : ViewModelBase
{
    [ObservableProperty] private string? _ffmpegPath;
    [ObservableProperty] private string? _videoPath;
    [ObservableProperty] private bool _showOptions;
    [ObservableProperty] private bool _autoStart;
    [ObservableProperty] private bool _check60MinTask;
    
    [RelayCommand]
    private async Task ToggleOptions()
    {
        WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("任务完成啦！"));
        await ConfigManager.SaveConfigAsync(ConfigType.ShowAsOption,ShowOptions);
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
            await ConfigManager.SaveConfigAsync(ConfigType.FfmpegPath,FfmpegPath);
        }
    }
    
    [RelayCommand]
    private async Task PickVideoPathAsync()
    {
        if (string.IsNullOrWhiteSpace(FfmpegPath))
        {
            Debug.WriteLine("ffmpeg path is empty.");
            return;
        }
        var pickFile = await FolderPickHelper.PickFileAsync("Choose Video Path",[".mp4",".flv",".mkv",".mov",".avi"]);
        if (string.IsNullOrWhiteSpace(pickFile))
            return;
        if (!await FfmpegWrapper.CheckVideoAvailableAsync(FfmpegPath,pickFile))
        {
            Debug.WriteLine("video is not available.");
            return;
        }
     
        VideoPath = pickFile;
        await ConfigManager.SaveConfigAsync(ConfigType.VideoPath,VideoPath);
    }
    
    [RelayCommand]
    private async Task AutoStartOptionAsync()
    {
        await ConfigManager.SaveConfigAsync(ConfigType.AutoStart,AutoStart);
    }
    
    [RelayCommand]
    private async Task Check60MinTaskAsync()
    {
        await ConfigManager.SaveConfigAsync(ConfigType.Check60MinTask,Check60MinTask);
    }
}