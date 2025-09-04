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
                Arguments = $"-v error -i \"{videoPath}\" -f null -",
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
    
}