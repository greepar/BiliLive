using System.Text.Json;
using System.Threading.Tasks;
using BiliLive.Core.Models.BiliService;

namespace BiliLive.Core.Interface;

public interface IBiliService
{
    //状态相关
    bool IsLogged { get; }
    
    // 登录相关
    Task<LoginResult> LoginAsync(string? biliCookie = null);
    Task<QrLoginInfo?> GetLoginUrlAsync();
    Task<int?> GeQrStatusCodeAsync(string qrCodeKey);
    

    // 直播相关
    Task<LiveRoomInfo> GetRoomInfoAsync();
    Task<JsonElement> StartLiveAsync();
}