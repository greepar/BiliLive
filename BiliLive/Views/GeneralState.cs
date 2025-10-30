using System;
using System.IO;
using Avalonia.Platform;
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
    
    public static void ClearState()
    {
        State.IsLogin = false;
        State.UserName = "未登录";
        using var defaultFace = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/userPic.jpg"));
        using var ms = new MemoryStream();
        defaultFace.CopyTo(ms);
        State.UserFaceByte = ms.ToArray();
        State.UserId = null;
        State.RoomId = null;
        State.IsStreaming = false;
        State.StreamTime = null;
    }
}