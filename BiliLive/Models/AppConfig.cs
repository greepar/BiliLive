using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BiliLive.Models;

[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull, WriteIndented = true)]
[JsonSerializable(typeof(AppConfig))]
public partial class SourceGenerateContext : JsonSerializerContext
{
}

public enum ConfigType
{
    BiliCookie,
    AutoStart,
    Check60MinTask,
    FfmpegPath,
    VideoPath,
    ShowAsOption,
}

public class AppConfig
{
    //string
    public string? BiliCookie { get; set; }
    public string? FfmpegPath { get; set; }
    public string? VideoPath { get; set; }
    
    //bool
    public bool AutoStart { get; set; }
    public bool Check60MinTask { get; set; }
    public bool ShowAsOption { get; set; }

    private static readonly Dictionary<ConfigType, Action<AppConfig, object?>> ConfigSetters
        = new()
        {
            [ConfigType.BiliCookie] = (cfg, val) => cfg.BiliCookie = val as string,
            [ConfigType.FfmpegPath] = (cfg, val) => cfg.FfmpegPath = val as string,
            [ConfigType.VideoPath] = (cfg, val) => cfg.VideoPath = val as string,
            
            [ConfigType.AutoStart] = (cfg, val) => cfg.AutoStart = val is true,
            [ConfigType.Check60MinTask] = (cfg, val) => cfg.Check60MinTask = val is true,
            [ConfigType.ShowAsOption] = (cfg, val) => cfg.ShowAsOption = val is true,
        };  

    // 修改对应属性
    public void SetConfig(ConfigType type, object? value)
    {
        if (ConfigSetters.TryGetValue(type, out var setter))
            setter(this, value);
        else
            throw new ArgumentException($"Unknown ConfigType: {type}");
    }
}

