using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliLive.Views.AccountWindow;

public partial class AccountWindowViewModel : ViewModelBase
{
    
    [ObservableProperty]private string _windowTitle = "Accounts";
    [ObservableProperty]private object _currentPage = new Pages.AccountPage();
    [ObservableProperty]private bool _showAddBtn = true;

    
    [RelayCommand]
    private void SwitchLoginPage()
    {
        CurrentPage = new Pages.QrLoginPage();
        WindowTitle = "Scan the QR to Login";
        ShowAddBtn = false;
    }
    
    
}