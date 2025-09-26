using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BiliLive.Models;

namespace BiliLive.Utils;

public static class ConfigManager
{
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
    
    public static async Task SaveAltSettingsAsync(AltSettings alt)
    {
        AppConfig existConfig = await LoadConfigAsync() ?? new AppConfig();

        if (existConfig.Alts == null)
        {
            //没有alts数组 将新建一个
            existConfig.Alts = [alt];
        }
        else
        {
            //存在alt
            var index = Array.FindIndex(existConfig.Alts, a => a.UserName == alt.UserName);
            if (index == -1)
            {
                //但是未找到 将添加
                var list = existConfig.Alts.ToList();
                list.Add(alt);
                existConfig.Alts = list.ToArray();
            }
            else
            {
                //找到对应alt 将替换
                existConfig.Alts[index] = alt;
            }
        }
        
        //保存配置
        var configJsonString = JsonSerializer.Serialize(existConfig, SourceGenerateContext.Default.AppConfig);
        await File.WriteAllTextAsync(ConfigFilePath, configJsonString);
    }
    
    public static async Task RemoveAltSettingsAsync(AltSettings alt)
    {
        AppConfig existConfig = await LoadConfigAsync() ?? new AppConfig();

        if (existConfig.Alts == null)
            return;

        // 过滤掉要删除的 Alt
        existConfig.Alts = existConfig.Alts
            .Where(a => a != null && a.UserName != alt.UserName)
            .ToArray();

        // 如果数组空了，可以直接置 null
        if (existConfig.Alts.Length == 0)
            existConfig.Alts = null;
        
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