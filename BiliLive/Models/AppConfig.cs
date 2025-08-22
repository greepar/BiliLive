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
    OtherSetting
}

public class AppConfig
{
    public string? BiliCookie { get; set; }
    
    
    private static readonly Dictionary<ConfigType, Action<AppConfig, object?>> ConfigSetters
        = new()
        {
            [ConfigType.BiliCookie] = (cfg, val) => cfg.BiliCookie = val as string,
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

