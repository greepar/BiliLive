using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using BiliLive.Views;
using ViewLocator.Generator.Common;

namespace BiliLive;

[GenerateViewLocator]
public partial class ViewLocator : IDataTemplate
{
    public Control? Build(object? data)
    {
        if (data is null)
        {
            return null;
        }
        var type = data.GetType();
        
        return s_views.TryGetValue(type, out var func) ? func.Invoke() : throw new Exception($"Unable to create view for type: {type}");
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
