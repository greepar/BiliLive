using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using BiliLive.Views.MainWindow.Pages.AutoService.Components;

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
            CoreSettingsContent.IsVisible = true;
        }
    }
    
    private void TimePickerButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button timePickerButton || this.DataContext is not AutoServiceViewModel mainVm)
        {
            return;
        }
                
        if (timePickerButton.Flyout is not Flyout flyout)
        {
            return;
        }
                
        if (flyout.Content is not TimePickerView timePickerView)
        {
            return;
        }
                
        var timePickerVm = new TimePickerViewModel(mainVm.StartTime);
        timePickerView.DataContext = timePickerVm;
                
        void OnConfirm(TimeSpan newTime)
        {
            mainVm.StartTime = newTime;
            flyout.Hide();
            Cleanup();
        }
    
        void OnCancel()
        {
            flyout.Hide();
            Cleanup();
        }
        
        void Cleanup()
        { 
            timePickerVm.OnConfirm -= OnConfirm; 
            timePickerVm.OnCancel -= OnCancel;
        }
                
        timePickerVm.OnConfirm += OnConfirm;
        timePickerVm.OnCancel += OnCancel;
        flyout.ShowAt(timePickerButton);
    }
}