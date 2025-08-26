using CommunityToolkit.Mvvm.ComponentModel;

namespace BiliLive.Views.MainWindow.Controls;

public partial class AutoServiceViewModel : ViewModelBase
{
    [ObservableProperty] private bool _showOptions;
    [ObservableProperty] private bool _autoStart;

    public AutoServiceViewModel()
    {
    }
    
}