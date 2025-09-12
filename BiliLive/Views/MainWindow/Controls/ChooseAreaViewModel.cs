using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BiliLive.Views.MainWindow.Controls;

public partial class V1Areas : ObservableObject
{
    [ObservableProperty] private string _message;
    

    public V1Areas(string msg)
    {
        Message = msg;
    }
}

public partial class ChooseAreaViewModel : ViewModelBase
{
    [ObservableProperty] private bool _showWindow;
    [ObservableProperty] private ObservableCollection<NotificationItem> _v1Area =
    [
        //test icon
        // new("Welcome to BiliLive!", Geometry.Parse(MdIcons.Notice)),
        // new("Check for updates every week", Geometry.Parse(MdIcons.Check)),
        // new("Report issues on GitHub", Geometry.Parse(MdIcons.Error))
    ];
    
    
}