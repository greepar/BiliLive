using CommunityToolkit.Mvvm.ComponentModel;

namespace BiliLive.Views;


//TODO: 全局状态管理
public partial class GeneralState : ObservableObject
{
    [ObservableProperty]private bool _isLogin;
    [ObservableProperty]private string _userName = "未登录";
    [ObservableProperty]private byte[]? _userFaceByte;
    [ObservableProperty]private long? _userId;
    [ObservableProperty]private long? _roomId;
    
    //状态
    [ObservableProperty]private bool _isStreaming;
    [ObservableProperty]private int? _streamTime;
}

public static class General
{
    private static GeneralState? _instance;
    public static GeneralState State => _instance ??= new GeneralState();
}