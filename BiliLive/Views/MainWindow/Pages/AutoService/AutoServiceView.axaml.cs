using System.Linq;
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

    private void TextBox_CheckNum(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox || string.IsNullOrEmpty(textBox.Text)) return;
        // 移除所有非数字字符
        var newText = new string(textBox.Text.Where(char.IsDigit).ToArray());
        if (newText == textBox.Text) return;
        textBox.Text = newText;
        textBox.CaretIndex = newText.Length;
    }
    // private void TimePickerButton_OnClick(object? sender, RoutedEventArgs e)
    // {
    //     if (sender is not Button timePickerButton || DataContext is not AutoServiceViewModel mainVm) return;
    //
    //     if (timePickerButton.Flyout is not Flyout flyout) return;
    //
    //     if (flyout.Content is not TimePickerView timePickerView) return;
    //
    //     var timePickerVm = new TimePickerViewModel(mainVm.StartTime);
    //     timePickerView.DataContext = timePickerVm;
    //
    //     void OnConfirm(TimeSpan newTime)
    //     {
    //         mainVm.StartTime = newTime;
    //         flyout.Hide();
    //         Cleanup();
    //     }
    //
    //     void OnCancel()
    //     {
    //         flyout.Hide();
    //         Cleanup();
    //     }
    //
    //     void Cleanup()
    //     {
    //         timePickerVm.OnConfirm -= OnConfirm;
    //         timePickerVm.OnCancel -= OnCancel;
    //     }
    //
    //     timePickerVm.OnConfirm += OnConfirm;
    //     timePickerVm.OnCancel += OnCancel;
    //     flyout.ShowAt(timePickerButton);
    // }
}