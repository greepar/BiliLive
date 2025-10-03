using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using Avalonia.Media;
using BiliLive.Models;
using BiliLive.Resources;
using BiliLive.Views.MainWindow;
using CommunityToolkit.Mvvm.Messaging;

namespace BiliLive.Utils;

public static class ConfigManager
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
    
    public static async Task SaveAltSettingsAsync(AltSettings alt)
    {
        AppConfig existConfig = await LoadConfigAsync() ?? new AppConfig();

        var index = existConfig.Alts.FindIndex(a => a.UserName == alt.UserName);
        if (index == -1)
            existConfig.Alts.Add(alt);
        else
            existConfig.Alts[index] = alt;
        
        //保存配置
        var configJsonString = JsonSerializer.Serialize(existConfig, SourceGenerateContext.Default.AppConfig);
        await File.WriteAllTextAsync(ConfigFilePath, configJsonString);
    }
    
    public static async Task RemoveAltSettingsAsync(AltSettings alt)
    {
        AppConfig existConfig = await LoadConfigAsync() ?? new AppConfig();

        existConfig.Alts.RemoveAll(a => a.UserName == alt.UserName);
        
        //保存配置
        var configJsonString = JsonSerializer.Serialize(existConfig, SourceGenerateContext.Default.AppConfig);
        await File.WriteAllTextAsync(ConfigFilePath, configJsonString);
    }
    
    public static async Task SaveConfigAsync(ConfigType configType, object? config)
    {
        AppConfig existConfig = await LoadConfigAsync() ?? new AppConfig();
        
        //设置对应属性
        existConfig.SetConfig(configType, config);
        
        //保存配置
        var configJsonString = JsonSerializer.Serialize(existConfig, SourceGenerateContext.Default.AppConfig);
        await File.WriteAllTextAsync(ConfigFilePath, configJsonString);
    }
    
    public static async Task<AppConfig> LoadConfigAsync()
    {
        if (!File.Exists(ConfigFilePath))
        {
            return new AppConfig();
        }
        
        var configString = await File.ReadAllTextAsync(ConfigFilePath);
        
        try
        {
            return JsonSerializer.Deserialize<AppConfig>(configString, SourceGenerateContext.Default.AppConfig) ?? new AppConfig();
        }
        catch (JsonException)
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("配置文件损坏，已重置为默认配置", Geometry.Parse(MdIcons.Error)));
            var config = new AppConfig();
            
            //保存配置格式
            var configJsonString = JsonSerializer.Serialize(config, SourceGenerateContext.Default.AppConfig);
            await File.WriteAllTextAsync(ConfigFilePath, configJsonString);
            
            return config;
        }
    }
}