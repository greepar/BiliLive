using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
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
    
    
    private void TextBox_CheckNum(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox || string.IsNullOrEmpty(textBox.Text)) return;
        // 移除所有非数字字符
        var newText = new string(textBox.Text.Where(char.IsDigit).ToArray());
        if (newText == textBox.Text) return;
        textBox.Text = newText;
        textBox.CaretIndex = newText.Length;
    }
}