using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

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

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        // if (string.IsNullOrWhiteSpace(MainBorder.Height))
        // {
        //     
        // }
        Console.WriteLine(MainBorder.Height);
        if (MainBorder.Height - 35 > 0 || ToggleSwitch.IsChecked == false)
        {
            MainBorder.Height = 35;
        }
        else
        {
            MainBorder.Height = 140;
        }
    }
}