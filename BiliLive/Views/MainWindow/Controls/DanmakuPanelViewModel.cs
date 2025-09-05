using System.Threading.Tasks;
using Avalonia.Media;
using BiliLive.Models;
using BiliLive.Resources;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BiliLive.Views.MainWindow.Controls;

public partial class DanmakuPanelViewModel : ViewModelBase
{
    
    [ObservableProperty] private bool _isDanmakuVisible = true;
    
    [RelayCommand]
    private static void SeparatePanel()
    {
        WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("暂未实现",Geometry.Parse(MdIcons.Check)));
    }
}