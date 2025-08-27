using Avalonia.Controls;

namespace BiliLive.Views.MainWindow.Controls;

public partial class AutoServiceView : UserControl
{
    public AutoServiceView()
    {
        if (Design.IsDesignMode)
        {
            DataContext = new AutoServiceViewModel();
        }
        InitializeComponent();
    }

    // public AutoServiceControl(AutoServiceControlViewModel vm) : this()
    // {
    //     DataContext = vm;
    // }
    
}