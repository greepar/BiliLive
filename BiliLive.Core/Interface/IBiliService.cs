using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BiliLive.Core.Models.BiliService;
using BiliLive.Core.Services.BiliService;

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
    
    Task<JsonElement> GetLiveDataAsync(string liveKey);

    Task ChangeRoomTitleAsync(string title);
    // Task ChangeRoomAreaAsync(int areaId);
    // Task ChangeRoomCoverAsync(byte[] cover);
    
    Task<JsonElement> StartLiveAsync();
}

public class BiliServiceImpl : IBiliService
{
    public bool IsLogged  { get; private set; }

    public async Task<LoginResult> LoginAsync(string? biliCookie = null)
    {
        var loginResult = await _loginService.LoginAsync(biliCookie);
        if (loginResult is LoginSuccess) { IsLogged = true; }
        return loginResult;
    }
    
    public async Task<QrLoginInfo?> GetLoginUrlAsync() => await _loginService.GetLoginUrlAsync();
    public async Task<int?> GeQrStatusCodeAsync(string qrCodeKey) => await _loginService.GeQrStatusCodeAsync(qrCodeKey);
    public async Task<LiveRoomInfo> GetRoomInfoAsync() => await _liveService.GetRoomInfoAsync();
    public async Task<JsonElement> StartLiveAsync() => await _liveService.StartLiveAsync();
    
    public async Task<JsonElement> GetLiveDataAsync(string apiKey) => await _liveService.GetLiveDataAsync(apiKey);

    public async Task ChangeRoomTitleAsync(string title) => await _liveService.ChangeRoomInfoAsync("title", title);

    public async Task<string?> StopLiveAsync() => await _liveService.StopLiveAsync();
    
    
    //构造初始值
    private const string UserAgent =
        "LiveHime/7.23.0.9579 os/Windows pc_app/livehime build/9579 osVer/10.0_x86_64";
    private readonly LoginService _loginService;
    private readonly LiveService _liveService;
    
    public BiliServiceImpl()
    {
        //初始化 HttpClient 和 CookieContainer
        var cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = cookieContainer
        };
        var httpClient = new HttpClient(handler);
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        
        //构造子服务
        _liveService = new LiveService(httpClient, cookieContainer);
        _loginService = new LoginService(httpClient, cookieContainer);
    }

}