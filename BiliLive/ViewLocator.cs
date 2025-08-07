using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using BiliLive.ViewModels;
using BiliLive.Views;

namespace BiliLive;

public class ViewLocator : IDataTemplate
{
    private static readonly Dictionary<Type, Func<Control>> ViewFactoryMap = new()
    {
        [typeof(MainWindowViewModel)] = () => new MainWindow()
        // 继续添加你的 ViewModel/View 映射
    };
    

    public Control? Build(object? param)
    {
        if (param == null)
            return null;

        var vmType = param.GetType();

        if (ViewFactoryMap.TryGetValue(vmType, out var viewFactory))
        {
            return viewFactory();
        }

        return new TextBlock { Text = $"View not found for: {vmType.Name}" };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}