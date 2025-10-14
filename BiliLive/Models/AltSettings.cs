using System.Collections.Generic;
using BiliLive.Core.Models.BiliService;

namespace BiliLive.Models;


public class AltSettings
{
    public required string CookieString { get; set; }
    
    public required string UserName { get; set; }
    public string[]? DanmakuList { get; set; } = null;
    public bool IsSendGift { get; set; }
    
    public ProxyInfo? ProxyInfo { get; set; }
    
    public List<string>? DanmakuTextList {get; set;}
}