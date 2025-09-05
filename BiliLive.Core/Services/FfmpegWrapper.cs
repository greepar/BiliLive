using System;
using System.Diagnostics;
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
            
            string output = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();
            if (string.IsNullOrWhiteSpace(output))
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
    
    public static async Task<bool> StartStreamingAsync(string ffmpegPath, string videoPath, string rtmpUrl,string apiKey)
    {
        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                //直播参数
                Arguments = "",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();

            // string output = await process.StandardOutput.ReadToEndAsync();
            string errorOutput = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();
            if (process.ExitCode == 0)
            {
                return true;
            }
            Debug.WriteLine($"Ffmpeg error output: {errorOutput}");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error starting streaming: {ex.Message}");
            return false;
        }
    }
}