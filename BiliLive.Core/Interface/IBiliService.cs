using System.Diagnostics;
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
    long RoomId { get; }
    
    // 登录相关
    Task<LoginResult> LoginAsync(string? biliCookie = null);
    Task<QrLoginInfo?> GetLoginUrlAsync();
    Task<int?> GeQrStatusCodeAsync(string qrCodeKey);
    
    // 直播相关
    Task<JsonElement> StartLiveAsync();
    Task StopLiveAsync();
    Task<JsonElement> GetAreasListAsync();
    Task<JsonElement> GetMyLastChooseAreaAsync();
    Task<LiveRoomInfo> GetRoomInfoAsync();
    Task<JsonElement> GetLiveDataAsync(string liveKey);

    Task ChangeRoomTitleAsync(string title);
    Task ChangeRoomAreaAsync(int areaId);
    
    Task ChangeRoomCoverAsync(byte[] coverImage);
    

}

public class BiliServiceImpl : IBiliService
{
    public bool IsLogged  { get; private set; }
    public long RoomId  { get; private set; }
    
    //登录相关
    public async Task<LoginResult> LoginAsync(string? biliCookie = null)
    {
        var loginResult = await _loginService.LoginAsync(biliCookie);
        if (loginResult is LoginSuccess) { IsLogged = true; }

        var roomInfo = await _liveService.GetRoomInfoAsync();
        RoomId = roomInfo.RoomId;
        return loginResult;
    }
    
    public async Task<QrLoginInfo?> GetLoginUrlAsync() => await _loginService.GetLoginUrlAsync();
    public async Task<int?> GeQrStatusCodeAsync(string qrCodeKey) => await _loginService.GeQrStatusCodeAsync(qrCodeKey);
    
    //直播相关
    public async Task<JsonElement> StartLiveAsync() => await _liveService.StartLiveAsync();
    public async Task StopLiveAsync() => await _liveService.StopLiveAsync();
    public async Task<JsonElement> GetAreasListAsync() => await _liveService.GetAreasListAsync();

    public async Task<LiveRoomInfo> GetRoomInfoAsync() => await _liveService.GetRoomInfoAsync();
    
    public async Task<JsonElement> GetMyLastChooseAreaAsync() => await _liveService.GetMyLastChooseAreaAsync();
    
    public async Task<JsonElement> GetLiveDataAsync(string apiKey) => await _liveService.GetLiveDataAsync(apiKey);

    public async Task ChangeRoomTitleAsync(string title) => await _liveService.ChangeRoomInfoAsync(LiveService.ChangeType.Title, title);
    public async Task ChangeRoomAreaAsync(int area) => await _liveService.ChangeRoomInfoAsync(LiveService.ChangeType.Area, area);
    public async Task ChangeRoomCoverAsync(byte[] imageBytes) => await _liveService.ChangeRoomInfoAsync(LiveService.ChangeType.Cover, imageBytes);

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