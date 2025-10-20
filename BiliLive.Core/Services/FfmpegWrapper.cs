using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BiliLive.Core.Services;

public static class FfmpegWrapper
{
    public static async Task<bool> CheckFfmpegAvailableAsync(string ffmpegPath)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = "-version",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();

            string output = await process.StandardOutput.ReadToEndAsync();

            await process.WaitForExitAsync();
            if (output.Contains("ffmpeg version"))
            {
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking ffmpeg: {ex.Message}");
            return false;
        }
    }

    public static async Task<bool> CheckVideoAvailableAsync(string ffmpegPath,string videoPath)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-v error -i \"{videoPath}\" -t 5 -f null -",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();
            
            var output = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            return !string.IsNullOrWhiteSpace(output);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error checking ffmpeg: {ex.Message}");
            return false;
        }
    }
    
    private static Process? _streamFfmpegProcess;
    private static CancellationTokenSource? _streamCts;
    public static async Task StartStreamingAsync(string ffmpegPath, string videoPath, string streamUrl,string apiKey,int seconds = 5)
    {
        if (_streamCts == null)
        { 
            _streamCts = new CancellationTokenSource();
        }else{ throw new Exception("已有推流任务在进行中，请先中断当前推流任务"); }
        
        var token = _streamCts.Token;
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                //TODO:待隐藏窗口参数
                FileName = ffmpegPath,
                //直播参数
                Arguments = $"-stream_loop -1 -re -i \"{videoPath}\" -c:v libx264 -preset veryfast -tune zerolatency -profile:v baseline -b:v 300k -maxrate 300k -bufsize 600k -vf \"scale=-2:720,fps=30\" -c:a aac -b:a 128k -ar 44100 -ac 2 -t {seconds} -f flv \"{streamUrl}/{apiKey}\"",
                UseShellExecute = false,
                RedirectStandardError = true
            };

            var errorOutputBuilder = new StringBuilder();
       
            _streamFfmpegProcess = new Process();
            _streamFfmpegProcess.StartInfo = processStartInfo;
            _streamFfmpegProcess.Start();
            // 读取错误输出
            _streamFfmpegProcess.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorOutputBuilder.AppendLine(e.Data);
                }
            };
            _streamFfmpegProcess.BeginErrorReadLine();

            await _streamFfmpegProcess.WaitForExitAsync(token);
            if (_streamFfmpegProcess.ExitCode != 0)
            {
                // 处理非零退出代码
                if (_streamFfmpegProcess.ExitCode is -1 or 1 or -1073741510)
                {
                    // 用户主动关闭
                    throw new Exception($"Ffmpeg被手动关闭. \n Exit code: {_streamFfmpegProcess.ExitCode}");
                }
                throw new Exception($"Ffmpeg其他异常退出：{_streamFfmpegProcess.ExitCode}. \n Ffmpeg输出:{errorOutputBuilder}");
            }

        }
        catch(OperationCanceledException)
        {
            // 取消操作时关闭Ffmpeg进程
            if (_streamFfmpegProcess is { HasExited: false })
            {
                _streamFfmpegProcess.Kill();
            }
        }
        catch(Exception ex)
        {
            _streamCts.Dispose();
            _streamCts = null;
            throw new Exception($"Ffmpeg streaming error : \n {ex.Message}");
        }
    }
    
    public static async Task InterruptStreamingAsync()
    {
        if (_streamCts != null)
        {
            await _streamCts.CancelAsync();
            _streamCts.Dispose();
            _streamCts = null;
        }
    }
}