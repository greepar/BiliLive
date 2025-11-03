using CommunityToolkit.Mvvm.ComponentModel;

namespace BiliLive.Views.MainWindow.Pages.AutoService.Components.Utility;

public partial class ClockTickViewModel(int value, string display) : ObservableObject
{
    [ObservableProperty] private string _display = display;
    [ObservableProperty] private int _value = value;
    
    [ObservableProperty]
    private bool _isSelected;
}