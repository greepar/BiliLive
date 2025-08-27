using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;

namespace BiliLive.Services;

public static class FolderPickHelper
{
    public static async Task<string?> PickFolderAsync(string title = "选择文件夹")
    {
        TopLevel? topLevel = null;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
        {
            topLevel = window;
        }
        else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewApp &&
                 singleViewApp.MainView?.GetVisualRoot() is TopLevel root)
        {
            topLevel = root;
        }

        if (topLevel == null || !topLevel.StorageProvider.CanPickFolder)
            return null;

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = title,
            AllowMultiple = false
        });

        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }

    public static async Task<string?> PickFileAsync(string title = "选择文件",string[]? extensions = null)
    {
        TopLevel? topLevel = null;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime { MainWindow: { } window })
        {
            topLevel = window;
        }
        else if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleViewApp &&
                 singleViewApp.MainView?.GetVisualRoot() is TopLevel root)
        {
            topLevel = root;
        }

        if (topLevel == null || !topLevel.StorageProvider.CanPickFolder)
            return null;

        // 弹出浏览文件夹对话框
        var filters = extensions is { Length: > 0 }
            ? new[]
            {
                new FilePickerFileType("自定义文件")
                {
                    Patterns = extensions.Select(e => $"*.{e.TrimStart('.')}").ToArray()
                }
            }
            : null;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = title,
            AllowMultiple = false,
            FileTypeFilter = filters
        });

        return files.Count > 0 ? files[0].Path.LocalPath : null;
    }
}