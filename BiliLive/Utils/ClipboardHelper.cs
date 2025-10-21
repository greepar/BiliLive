using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;

namespace BiliLive.Utils;

public static class ClipboardHelper
{
    public static IClipboard Get()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
        {
            return window.Clipboard!;
        }

        if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime viewApp &&
            viewApp.MainView?.GetVisualRoot() is TopLevel topLevel)
        {
            return topLevel.Clipboard!;
        }

        return null!;
    }
}