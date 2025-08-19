using System;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media.Imaging;
using BiliLive.Services;
using BiliLive.Views.AccountWindow;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliLive.Views.MainWindow;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly AccountInterface? accInterface;
    
    [ObservableProperty] private string? _userName = "Not Login";
    [ObservableProperty] private string? _roomTitle;
    [ObservableProperty] private string? _roomArea = "选择分区";
    [ObservableProperty] private string? _apiKey = "Null";
    [ObservableProperty] private string? _ffmpegPath;
    [ObservableProperty] private string? _videoPath;
    [ObservableProperty] private string? _status = "Not Login";
    [ObservableProperty] private string? _btnWord = "Start Stream";
    [ObservableProperty] private Bitmap? _userFace ;
    [ObservableProperty] private Bitmap? _roomCover ;
    [ObservableProperty] private bool _autoStart = false ;
    [ObservableProperty] private bool _isLoginOpen = false ;
    [ObservableProperty] private bool _checkTask = false ;
    [ObservableProperty] private bool _streamBtn = false ;
    [ObservableProperty] private long? _userId = 196431435;
    
    public MainWindowViewModel()
    {
        
    }

    public MainWindowViewModel(AccountInterface accountInterface)
    {
        accInterface = accountInterface;
    }
    
    [RelayCommand]
    private async Task LoginAsync()
    {
        await accInterface.LoginAsync();
        UserName = "UserName";
        UserId = 123;
    }

    [RelayCommand]
    private async Task StartServiceAsync()
    {
        await Task.Delay(1);
        UserName = "Hello";
    }

    [RelayCommand]
    private async Task ChangeAreaAsync()
    {
        await Task.Delay(1);
        UserName = "HelloArea";
    }
  
}