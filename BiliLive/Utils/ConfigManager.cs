using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using BiliLive.Models;
using BiliLive.Resources;
using BiliLive.Views.MainWindow;
using CommunityToolkit.Mvvm.Messaging;

namespace BiliLive.Utils;

public static class ConfigManager
{
     private static readonly string ConfigFilePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    private static readonly Channel<AppConfig> SaveChannel = Channel.CreateUnbounded<AppConfig>(
        new UnboundedChannelOptions
        {
            SingleReader = true, // 单消费者，提高性能
            SingleWriter = false
        });

    private static readonly Task SaveWorker = Task.Run(ProcessQueue); // 初始化类时即启动后台任务
    private static readonly SemaphoreSlim ProcessLock = new(1, 1); // 避免读写冲突
    private static AppConfig? _cachedConfig;
    
    public static async Task SaveConfigAsync(ConfigType configType, object? config)
    {
        var existConfig = await LoadConfigAsync();
        existConfig.SetConfig(configType, config);

        // 先写入缓存
        _cachedConfig = existConfig;
        // 异步消费写入硬盘
        await SaveChannel.Writer.WriteAsync(existConfig);
    }

    // 读取配置（带锁，防止同时写入）
    public static async Task<AppConfig> LoadConfigAsync()
    {
        await ProcessLock.WaitAsync();
        try
        {
            // 如果存在缓存配置
            if (_cachedConfig != null) { return _cachedConfig; }
            // 如果配置文件不存在，返回默认配置
            if (!File.Exists(ConfigFilePath))
                return new AppConfig();
            // 读取配置文件
            var configString = await File.ReadAllTextAsync(ConfigFilePath);
            _cachedConfig = JsonSerializer.Deserialize<AppConfig>(configString, SourceGenerateContext.Default.AppConfig)
                            ?? new AppConfig();
            return _cachedConfig;
        }
        catch (JsonException)
        {
            Dispatcher.UIThread.Invoke(() => { WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("配置文件损坏，已重置为默认配置", Geometry.Parse(MdIcons.Error))); });
            var config = new AppConfig();
            var json = JsonSerializer.Serialize(config, SourceGenerateContext.Default.AppConfig);
            await File.WriteAllTextAsync(ConfigFilePath, json);
            return config;
        }
        finally
        {
            ProcessLock.Release();
        }
    }
    
    //Alt相关
    public static async Task SaveAltSettingsAsync(AltSettings alt)
    {
        var existConfig = await LoadConfigAsync();
        var index = existConfig.Alts.FindIndex(a => a.UserName == alt.UserName);
        if (index == -1)
            existConfig.Alts.Add(alt);
        else
            existConfig.Alts[index] = alt;
        
        //保存配置
        _cachedConfig = existConfig;
        await SaveChannel.Writer.WriteAsync(existConfig);
    }
    
    public static async Task RemoveAltSettingsAsync(AltSettings alt)
    {
        var existConfig = await LoadConfigAsync();
        existConfig.Alts.RemoveAll(a => a.UserName == alt.UserName);
        
        //保存配置
        _cachedConfig = existConfig;
        await SaveChannel.Writer.WriteAsync(existConfig);
    }
    
    // 在应用退出时调用，确保队列清空 
    public static async Task ShutdownAsync()
    {
        SaveChannel.Writer.Complete();
        await SaveWorker;
    }
    
    // 异步后台任务，统一写文件
    private static async Task ProcessQueue()
    {
        var delay = TimeSpan.FromSeconds(1); // 写入防抖：合并 500ms 内多次修改
    
        await foreach (var newConfig in SaveChannel.Reader.ReadAllAsync())
        {
            var pending = newConfig;
            
            // 等待 1秒，看是否有更多修改进来
            await Task.Delay(delay);
            // 检查是否有新任务叠上来了
            while (SaveChannel.Reader.TryRead(out var next))
            {
                pending = next;
            }

            try
            {
                var json = JsonSerializer.Serialize(pending, SourceGenerateContext.Default.AppConfig);
                await ProcessLock.WaitAsync(); // 确保不会和 Load 冲突
                await File.WriteAllTextAsync(ConfigFilePath, json);
            }
            catch (Exception ex)
            {
                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await ShowWindowHelper.ShowErrorAsync("保存配置时发生错误：" + ex.Message);
                });
            }
            finally
            {
                ProcessLock.Release();
            }
        }
    }
}