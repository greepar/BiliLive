using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BiliLive.Core.Models.BiliService;
using BiliLive.Core.Services.BiliService;

namespace BiliLive.Core.Interface;

public interface IAltService : IDisposable
{
    // 登录相关
    Task<LoginResult> LoginAsync(string? biliCookie);
    Task<QrLoginInfo?> GetLoginUrlAsync();
    Task<int?> GeQrStatusCodeAsync(string qrCodeKey);
    
    //小号相关
    Task SendDanmakuAsync(string message,string roomId);
    Task SendGiftAsync(string roomId);
    Task TryAddNewProxy(ProxyInfo proxyInfo);
}

public class AltServiceImpl : IAltService
{
    // 登录相关
    public async Task<LoginResult> LoginAsync(string? biliCookie) => await _loginService.LoginAsync(biliCookie);
    public async Task<QrLoginInfo?> GetLoginUrlAsync() => await _loginService.GetLoginUrlAsync();
    public async Task<int?> GeQrStatusCodeAsync(string qrCodeKey) => await _loginService.GeQrStatusCodeAsync(qrCodeKey);
    //小号相关
    public async Task SendDanmakuAsync(string message,string roomId) => await _altService.SendDanmakuAsync(message,roomId);
    public async Task SendGiftAsync(string roomId) => await _altService.SendGiftAsync(roomId);
    public async Task TryAddNewProxy(ProxyInfo proxyInfo)
    {
        var proxy = new WebProxy(proxyInfo.ProxyAddress);
        if (!string.IsNullOrWhiteSpace(proxyInfo.Username) && !string.IsNullOrWhiteSpace(proxyInfo.Password))
        {
            proxy.Credentials = new NetworkCredential(proxyInfo.Username, proxyInfo.Password);
        }
        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = _cookieContainer,
            UseProxy = true,
            Proxy = proxy
        };
        _httpClient.Dispose();
        var newHttpClient = new HttpClient(handler, disposeHandler: true);
        newHttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        _httpClient = null!;
        _httpClient = newHttpClient;
        _altService = new AltService(_httpClient,_cookieContainer);
         
        // 测试代理是否可用
        var responseSting = await _httpClient.GetAsync(new Uri("https://api.bilibili.com/x/web-interface/nav"));
        if (!responseSting.IsSuccessStatusCode)
        {
            throw new Exception("代理不可用");
        }
    }
    
    //释放
    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
    
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0";
    private HttpClient _httpClient;
    private AltService _altService;
    private readonly LoginService _loginService;
    private readonly CookieContainer _cookieContainer= new ();
    
    public AltServiceImpl(string biliCookie = "",ProxyInfo? proxyInfo = null)
    {
        var cookiePairs = biliCookie.Split(';');
        foreach (var pair in cookiePairs)
        {
            var cookieParts = pair.Split('=', 2);
            if (cookieParts.Length != 2) continue;

            var name = cookieParts[0].Trim();
            var value = cookieParts[1].Trim();

            _cookieContainer.Add(new Cookie(name, value)
            {
                Domain = ".bilibili.com",
                Path = "/"
            });
        }
        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = _cookieContainer,
        };
        
        if (proxyInfo != null)
        {
            var proxy = new WebProxy(proxyInfo.ProxyAddress);
            if (!string.IsNullOrWhiteSpace(proxyInfo.Username) && !string.IsNullOrWhiteSpace(proxyInfo.Password))
            {
                proxy.Credentials = new NetworkCredential(proxyInfo.Username, proxyInfo.Password);
            }
            handler.UseProxy = true;
            handler.Proxy = proxy;
        }

        _httpClient = new HttpClient(handler, disposeHandler: true);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        
        _altService = new AltService(_httpClient, _cookieContainer);
        _loginService = new LoginService(_httpClient, _cookieContainer);
    }
}