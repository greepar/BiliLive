using System.Runtime.CompilerServices;
using BiliLive.Core.Models.BiliService;
using BiliLive.Views.MainWindow.Pages;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliLive.Views.AccountWindow;

public partial class AccountWindowViewModel : ViewModelBase
{
    private readonly LoginService? _loginService;
    
    [ObservableProperty]private string _windowTitle = "Accounts";
    [ObservableProperty]private object? _currentPage = new AccountPageView();
    [ObservableProperty]private bool _showAddBtn = true;
    [ObservableProperty]private LoginResult? _loginResult;

    public AccountWindowViewModel(){}
    public AccountWindowViewModel(LoginService loginService) : this()
    {
        _loginService = loginService;
    }
    
    [RelayCommand]
    private void SwitchLoginPage()
    {
        CurrentPage = new QrLoginPageView();
        WindowTitle = "Scan the QR to Login";
        ShowAddBtn = false;
    }
    
    
}