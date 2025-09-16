using Avalonia.Controls;
using Avalonia.Interactivity;

namespace BiliLive.Views.MainWindow.Controls;

public partial class AutoServiceView : UserControl
{
    public AutoServiceView()
    {
        if (Design.IsDesignMode) DataContext = new AutoServiceViewModel();
        InitializeComponent();
    }

    // public AutoServiceControl(AutoServiceControlViewModel vm) : this()
    // {
    //     DataContext = vm;
    // }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        // Console.WriteLine(MainBorder.Height);
        if (MainBorder.Height - 32 > 0)
            MainBorder.Height = 32;
        else
            MainBorder.Height = 115;
    }
}