using System;
using Avalonia.Controls;
using Avalonia.Interactivity;


namespace BiliLive.Views.MainWindow.Pages.AutoService;

public partial class AutoServiceView : UserControl
{
    public AutoServiceView()
    {
        if (Design.IsDesignMode) DataContext = new AutoServiceViewModel();
        InitializeComponent();
    }
    
    private void ExpandButton_OnClick(object? sender, RoutedEventArgs e)
    {
        // Console.WriteLine(MainBorder.Height);
        if (MainBorder.Height - 32 > 0)
        {
            MainBorder.Height = 32;
            // CoreSettingsCotent.IsVisible = false;
        }
        else
        {
            MainBorder.Height = 115;
            CoreSettingsCotent.IsVisible = true;
        }
    }
    
  

}