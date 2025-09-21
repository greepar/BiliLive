using CommunityToolkit.Mvvm.ComponentModel;

namespace BiliLive.Views.MainWindow.Pages.AutoService.Components.Utility;

public partial class ClockTickViewModel : ObservableObject
{
    [ObservableProperty] private string _display;
    [ObservableProperty] private int _value;
    
    [ObservableProperty]
    private bool _isSelected;

    public ClockTickViewModel(int value, string display)
    {
        _value = value;
        _display = display;
    }
}