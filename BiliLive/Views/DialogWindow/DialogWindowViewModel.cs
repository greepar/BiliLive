using CommunityToolkit.Mvvm.ComponentModel;

namespace BiliLive.Views.DialogWindow;

public partial class DialogWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string? _message;
}