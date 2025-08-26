using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BiliLive.Models;

namespace BiliLive.Utils;

public static class ConfigManager
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    public static async Task SaveConfigAsync(ConfigType configType, object? config)
    {
        AppConfig existConfig = await LoadConfigAsync() ?? new AppConfig();
        
        //设置对应属性
        existConfig.SetConfig(configType, config);
        
        //保存配置
        var configJsonString = JsonSerializer.Serialize(existConfig, SourceGenerateContext.Default.AppConfig);
        await File.WriteAllTextAsync(ConfigFilePath, configJsonString);
    }
    
    public static async Task<AppConfig?> LoadConfigAsync()
    {
        if (!File.Exists(ConfigFilePath))
        {
            return null;
        }
        var configString = await File.ReadAllTextAsync(ConfigFilePath);
        return JsonSerializer.Deserialize<AppConfig>(configString, SourceGenerateContext.Default.AppConfig);
    }
}