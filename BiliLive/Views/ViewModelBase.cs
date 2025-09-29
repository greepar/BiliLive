using System;
using System.Threading.Tasks;
using BiliLive.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace BiliLive.Views;

public class ViewModelBase : ObservableObject
{
    // protected async Task RunSafeAsync(Func<Task> action)
    // {
    //     try
    //     {
    //         await action();
    //     }
    //     catch (Exception ex)
    //     {
    //         var vm = new DialogWindow.DialogWindowViewModel
    //         {
    //             Message = ex.Message,
    //         };
    //         await ShowWindowHelper.ShowWindowAsync(new DialogWindow.DialogWindow(){DataContext = vm});
    //     }
    // }
    //
    // protected async Task<T?> RunSafeAsync<T>(Func<Task<T>> func)
    // {
    //     try
    //     {
    //         return await func();
    //     }
    //     catch (Exception ex)
    //     {
    //         var vm = new DialogWindow.DialogWindowViewModel
    //         {
    //             Message = ex.Message,
    //         };
    //         await ShowWindowHelper.ShowWindowAsync(new DialogWindow.DialogWindow(){DataContext = vm});
    //         return default; // 失败返回默认值
    //     }
    // }
}