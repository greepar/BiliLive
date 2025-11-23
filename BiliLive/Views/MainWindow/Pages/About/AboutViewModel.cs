using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliLive.Views.MainWindow.Pages.About;

public partial class AboutViewModel : ViewModelBase
{
    [ObservableProperty]private Bitmap _developerAvatar;
    // [ObservableProperty]private object _currentView = new DialControl();
    public AboutViewModel()
    {
        var file = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/userPic.jpg"));
        DeveloperAvatar = PicHelper.ResizeStreamToBitmap(file,120,120) ?? new Bitmap(file);
    }

#if DEBUG
    //调试模式
    
    [RelayCommand]
    private static void SetTopMostWindow(bool isTopMost)
    {
         AvaloniaUtils.SetTopMostWindow(isTopMost);
    }
    
    [RelayCommand]
    private static void OpenCurrentFolder()
    {
        var currentPath = AppDomain.CurrentDomain.BaseDirectory;
        Process.Start(new ProcessStartInfo
        {
            FileName = currentPath,
            UseShellExecute = true
        });
    }
    
#endif

}