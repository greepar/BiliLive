using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace BiliLive.Utils;

public static class AvaloniaUtils
{
    public static async Task OpenUrl(string url)
    {
        await TopLevel.Launcher.LaunchUriAsync(new Uri(url));
    }
    
    public static IClipboard GetClipboard()
    {
        return TopLevel.Clipboard ?? throw new InvalidOperationException("Top level is not set");
    }
    
    public static async void SwitchTheme(ThemeVariant themeVariant)
    {
        var app = App ?? throw new InvalidOperationException("Application is not set");
        app.RequestedThemeVariant = themeVariant;
    }
    public static async Task<string?> PickFileAsync(string title = "选择文件",string[]? extensions = null)
    {
        // 拓展名过滤器
        var filters = extensions is { Length: > 0 }
            ? new[]
            {
                new FilePickerFileType("自定义文件")
                {
                    Patterns = extensions.Select(e => $"*.{e.TrimStart('.')}").ToArray()
                }
            }
            : null;

        var files = await TopLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = filters
        });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }
    
    public static async Task<string?> PickFolderAsync(string title = "选择文件夹")
    {
        var folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }
    
    //私有方法
    private static Application? App => Application.Current;
    private static TopLevel TopLevel
    {
        get
        {
            return App?.ApplicationLifetime switch
            {
                IClassicDesktopStyleApplicationLifetime { MainWindow: { } window } => window,
                ISingleViewApplicationLifetime singleViewApp when singleViewApp.MainView?.GetVisualRoot() is TopLevel root => root,
                _ => throw new InvalidOperationException("Failed to resolve TopLevel")
            };
        }
    }
}