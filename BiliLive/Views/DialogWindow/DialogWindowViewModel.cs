using System;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliLive.Views.DialogWindow;

public partial class DialogWindowViewModel : ViewModelBase
{
    [ObservableProperty] private string? _message;
    [ObservableProperty] private bool _isConfirmed;
    
    [RelayCommand]
    private void Confirm()
    {
        IsConfirmed = true;
    }
}

public class ErrorDialogWindowViewModel : DialogWindowViewModel
{
    
}

public partial class QrDialogViewModel : ViewModelBase , IDisposable
{
    [ObservableProperty] private Bitmap? _qrImage;

    public void Dispose()
    {
        QrImage?.Dispose();
        
        GC.SuppressFinalize(this);
    }
}