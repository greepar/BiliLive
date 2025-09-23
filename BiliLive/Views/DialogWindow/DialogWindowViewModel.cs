using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliLive.Views.DialogWindow;

public partial class DialogWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isConfirmed;
    
    [RelayCommand]
    private void Confirm()
    {
        IsConfirmed = true;
    }
}

public class ErrorDialogWindowViewModel : DialogWindowViewModel
{
    
}