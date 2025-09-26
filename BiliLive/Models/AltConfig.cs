namespace BiliLive.Models;


public class AltConfig
{
    public required string CookieString { get; set; }
    
    public required string UserName { get; set; }
    public string[]? DanmakuList { get; set; } = null;
    public bool IsSendGift { get; set; }
    
    public string? ProxyAddress { get; set; }
    public string? ProxyUsername { get; set; }
    public string? ProxyPassword { get; set; }
    
}