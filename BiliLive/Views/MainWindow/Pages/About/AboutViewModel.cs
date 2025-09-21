using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Resources.Controls;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BiliLive.Views.MainWindow.Pages.About;

public partial class AboutViewModel : ViewModelBase
{
    [ObservableProperty]private Bitmap _developerAvatar;
    [ObservableProperty]private object _currentView = new DialControl();
    public AboutViewModel()
    {
        // var file = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/userPic.jpg"));
        // DeveloperAvatar = PicHelper.ResizeStreamToBitmap(file,120,120) ?? new Bitmap(file);
    }
}