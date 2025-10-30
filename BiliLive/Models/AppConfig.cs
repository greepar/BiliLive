using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using BiliLive.Core.Models.BiliService;

namespace BiliLive.Models;

[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,WriteIndented = true)]
[JsonSerializable(typeof(AppConfig))]
[JsonSerializable(typeof(ProxyInfo))]
[JsonSerializable(typeof(AltSettings))]
public partial class SourceGenerateContext : JsonSerializerContext;

public enum ConfigType
{
    BiliCookie,
    
    AutoStart,
    StreamTime,
    RandomDelay,
    Check60MinTask,
    
    FfmpegPath,
    VideoPath,
    
    AutoSendGift,
    
    AutoClaimReward,
    TaskId
}

public class AppConfig
{
    //string
    public string? BiliCookie { get; set; }
    public string? FfmpegPath { get; set; }
    public string? VideoPath { get; set; }
    public string? TaskId { get; set; }
    
    //Time
    public TimeSpan? StreamTime { get; set; }
    
    //bool
    public bool AutoStart { get; set; }
    public bool RandomDelay { get; set; }
    public bool Check60MinTask { get; set; }
    public bool AutoSendGift { get; set; }
    public bool AutoClaimReward { get; set; }
    
    //AltSettings
    public List<AltSettings> Alts { get; set; } = [];     

    private static readonly Dictionary<ConfigType, Action<AppConfig, object?>> ConfigSetters
        = new()
        {
            [ConfigType.BiliCookie] = (cfg, val) => cfg.BiliCookie = val as string,
            [ConfigType.FfmpegPath] = (cfg, val) => cfg.FfmpegPath = val as string,
            [ConfigType.VideoPath] = (cfg, val) => cfg.VideoPath = val as string,
            [ConfigType.TaskId] = (cfg, val) => cfg.TaskId = val as string,
            
            [ConfigType.StreamTime] = (cfg, val) => cfg.StreamTime = val as TimeSpan?,
            
            [ConfigType.AutoStart] = (cfg, val) => cfg.AutoStart = val is true,
            [ConfigType.Check60MinTask] = (cfg, val) => cfg.Check60MinTask = val is true,
            [ConfigType.AutoSendGift] = (cfg, val) => cfg.AutoSendGift = val is true,
            [ConfigType.AutoClaimReward] = (cfg, val) => cfg.AutoClaimReward = val is true,
            [ConfigType.RandomDelay] = (cfg, val) => cfg.RandomDelay = val is true,
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

