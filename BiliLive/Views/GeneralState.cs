using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliLive.Views;

public partial class GeneralState : ObservableObject
{
    [ObservableProperty]private bool _isLogin;
    [ObservableProperty]private string? _userName;
    [ObservableProperty]private long? _userId;
    [ObservableProperty]private long? _roomId;
    
    //状态
    [ObservableProperty]private bool _isStreaming;
    [ObservableProperty]private int? _streamTime;
}