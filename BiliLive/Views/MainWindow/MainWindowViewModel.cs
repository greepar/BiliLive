using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using BiliLive.Core.Models.BiliService;
using BiliLive.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliLive.Views.MainWindow;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly AccountInterface? _accInterface;
    
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
        _accInterface = accountInterface;
    }
    
    [RelayCommand]
    private async Task LoginAsync()
    {
        if (_accInterface != null)
        {
            var loginResult = await _accInterface.LoginAsync();
            if (loginResult is not null)
            {
                switch (loginResult)
                {
                    case LoginSuccess success:
                        UserName = success.UserName;
                        UserFace = new Bitmap(success.UserFaceUrl);
                        UserId = success.UserId;
                        break;
                    case LoginFailed failed:
                        if (failed.IsCanceled)
                        {
                        }
                        else
                        {
                        }
                        break;
                }
            }
        }
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