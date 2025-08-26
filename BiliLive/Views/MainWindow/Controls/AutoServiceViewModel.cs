using System.Threading.Tasks;
using BiliLive.Models;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliLive.Views.MainWindow.Controls;

public partial class AutoServiceViewModel : ViewModelBase
{
    [ObservableProperty] private bool _showOptions;
    [ObservableProperty] private bool _autoStart;
    [ObservableProperty] private bool _check60MinTask;
    

    public AutoServiceViewModel()
    {
    }
    
    
    [RelayCommand]
    private async Task ToggleOptions()
    {
        await ConfigManager.SaveConfigAsync(ConfigType.AutoStart,AutoStart);
        await ConfigManager.SaveConfigAsync(ConfigType.BiliCookie,"true");
    }

    [RelayCommand]
    private async Task Check60MinTaskAsync()
    {
        await ConfigManager.SaveConfigAsync(ConfigType.Check60MinTask,Check60MinTask);
    }
}