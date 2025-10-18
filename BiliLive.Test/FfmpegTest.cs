using BiliLive.Core.Services;
using NUnit.Framework;

namespace BiliLive.Test;

[TestFixture]
public class FfmpegTest
{
    private readonly string _currentPath = AppDomain.CurrentDomain.BaseDirectory;
    private string _ffmpegPath;
    private string _videoPath;
    private string _streamUrl;
    private string _streamKey;

    [SetUp]
    public void Setup()
    {
        _ffmpegPath = Path.Combine(_currentPath, "ffmpeg.exe");
        TestContext.Out.WriteLine($"设置Ffmpeg目录：{_ffmpegPath}");
        
        _videoPath = Path.Combine(_currentPath, "test.mp4");
        TestContext.Out.WriteLine($"设置视频目录目录：{_videoPath}");
        
        _streamUrl = "rtmp://live-push.bilivideo.com/live-bvc";
        TestContext.Out.WriteLine($"直播目录：{_streamUrl}");
        
        _streamKey = "?streamname=live_196431435_1850141&key=05ccedee9beb44f60aba7df7ec8ab84e&schedule=rtmp&pflag=2";
        TestContext.Out.WriteLine($"直播密钥：{_streamKey}");
        
        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(_ffmpegPath), Is.True, "ffmpeg.exe 未找到，请确保已将其'复制到输出目录'。");
            Assert.That(File.Exists(_videoPath), Is.True, "test.mp4 未找到，请确保已将其'复制到输出目录'。");
        });
    }
    
    [Test]
    public async Task CheckFfmpegAvailable()
    { 
        // Act
        bool result = await FfmpegWrapper.CheckFfmpegAvailableAsync(_ffmpegPath);
       
        // Assert
        Assert.That(result, Is.True, "Ffmpeg不可用");
    }
    
    [Test]
    public async Task CheckVideoAvailableTest()
    {
        // Act
        var result = await FfmpegWrapper.CheckVideoAvailableAsync(_ffmpegPath,_videoPath);
        // Assert
        Assert.That(result, Is.False, "无效视频文件测试未通过");
    }
    
    [Test]
    public async Task StartStreamingTest()
    {
        // _streamKey = "12";
        
        // Act 测试推流5s直播
        var result = false;
        try
        {
            await FfmpegWrapper.StartStreamingAsync(_ffmpegPath,_videoPath,_streamUrl,_streamKey,15);
            result = true;
        }
        catch (Exception e)
        {
            await TestContext.Out.WriteLineAsync($"推流异常：{e.Message}");
        }
        
        // Assert
        Assert.That(result, Is.True, "推流失败");
    }
    
    [Test]
    public async Task InterruptStreamTest()
    {
        // Act 测试推流5s直播
        var result = false;
        try
        {
            _ = FfmpegWrapper.StartStreamingAsync(_ffmpegPath,_videoPath,_streamUrl,_streamKey,15);
            await Task.Delay(2000);
            await FfmpegWrapper.InterruptStreamingAsync();
            result = true;
        }
        catch (Exception e)
        {
            await TestContext.Out.WriteLineAsync($"中断异常：{e.Message}");
        }
        // Assert
        Assert.That(result, Is.True, "中断推流失败");
    }
}
