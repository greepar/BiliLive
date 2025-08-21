using System; 
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using BiliLive.Core.Models.BiliService;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLive.Services;

public class AccountInterface(IServiceProvider serviceProvider)
{
    public async Task<LoginResult?> LoginAsync()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
            desktop.MainWindow is null)
        {
            return null;
        }
        var mainWindow = desktop.MainWindow;
        
        var tcs = new TaskCompletionSource<LoginResult?>();
        
        void OnMainWindowGotFocus(object? sender, EventArgs e)
        {
        }
        
        void OnChildClosed(object? sender, EventArgs e)
        {
            // 不论窗口如何关闭，都清理事件订阅
            mainWindow.GotFocus -= OnMainWindowGotFocus;
            
            // 检查ViewModel中是否有结果，如果没有，则认为是取消操作
            LoginResult? result = null;
         
            
            // 任务完成并传递结果
            tcs.TrySetResult(result);
        }
        
        //开始订阅上述事件
        mainWindow.GotFocus += OnMainWindowGotFocus;
        
       
        return await tcs.Task;
    }
}