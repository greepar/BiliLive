namespace BiliLive.Models;

public class AutoStreamOptions
{
    public bool AutoStart { get; set; } = false;
    public bool Check60Min { get; set; } = false;
    public string? FfmpegPath { get; set; }
    public string? VideoUrl { get; set; }
}