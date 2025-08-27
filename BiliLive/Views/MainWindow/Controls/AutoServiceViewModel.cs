using System.Threading.Tasks;
using BiliLive.Models;
using BiliLive.Services;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliLive.Views.MainWindow.Controls;

public partial class AutoServiceViewModel : ViewModelBase
{
    [ObservableProperty] private string? _ffmpegPath;
    [ObservableProperty] private string? _videoPath;
    [ObservableProperty] private bool _showOptions;
    [ObservableProperty] private bool _autoStart;
    [ObservableProperty] private bool _check60MinTask;
    

    public AutoServiceViewModel()
    {
    }
    
    
    [RelayCommand]
    private async Task ToggleOptions()
    {
        await ConfigManager.SaveConfigAsync(ConfigType.ShowAsOption,ShowOptions);
    }

    [RelayCommand]
    private async Task PickFfmpegPathAsync()
    {
        var pickFile = await FolderPickHelper.PickFileAsync("Choose Ffmpeg Path",[".exe"]);
        if (string.IsNullOrWhiteSpace(pickFile) || !pickFile.EndsWith(".exe"))
            return;
        FfmpegPath = pickFile;
        await ConfigManager.SaveConfigAsync(ConfigType.FfmpegPath,FfmpegPath);
    }
    
    [RelayCommand]
    private async Task PickVideoPathAsync()
    {
        var pickFile = await FolderPickHelper.PickFileAsync("Choose Video Path");
        if (string.IsNullOrWhiteSpace(pickFile))
            return;
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