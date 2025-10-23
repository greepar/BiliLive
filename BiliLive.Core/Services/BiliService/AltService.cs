﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BiliLive.Core.Interface;
using BiliLive.Core.Models.BiliService;

namespace BiliLive.Core.Services.BiliService;

public class AltService : IDisposable
{
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0";

    private LoginService _loginService;
    
    private HttpClient _httpClient;
    private readonly CookieContainer _cookieContainer= new ();
    private bool _disposed;
    
    private readonly IBiliService? _biliService;
    
    public AltService(IBiliService? biliService = null,string biliCookie = "",ProxyInfo? proxyInfo = null)
    {
        if (biliService is not null)
        {
            _biliService = biliService;
        }
        
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
        
        _loginService = new LoginService(_httpClient, _cookieContainer);
    }
    
    //测试用，获取当前IP
    // public async Task GetCurrentIp()
    // {
    //     var aAResponseSting = await _httpClient.GetAsync(new Uri("https://4.ipw.cn"));
    //     var rResponseContent = await aAResponseSting.Content.ReadAsStringAsync();
    //     Console.WriteLine(rResponseContent);
    // }
    
    //修改并测试Proxy
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
         _loginService = new LoginService(_httpClient, _cookieContainer);
         
        // 测试代理是否可用
         // await GetCurrentIp();
         var responseSting = await _httpClient.GetAsync(new Uri("https://api.bilibili.com/x/web-interface/nav"));
         if (!responseSting.IsSuccessStatusCode)
         {
             throw new Exception("代理不可用");
         }
    }
    
    public async Task<LoginResult> LoginAsync(string? biliCookie = null) => await _loginService.LoginAsync(biliCookie);
    public async Task<QrLoginInfo?> GetLoginUrlAsync() => await _loginService.GetLoginUrlAsync();
    public async Task<int?> GeQrStatusCodeAsync(string qrCodeKey) => await _loginService.GeQrStatusCodeAsync(qrCodeKey);
    
    
    public async Task SendDanmakuAsync(String message)
    { 
        if (_biliService == null) throw new Exception("BiliService未初始化");
        var formData = new Dictionary<string, string>
        {
            { "bubble", "0"  },
            { "msg", message },
            { "color", "16777215"  },
            { "mode", "1"  },
            { "room_type", "0"  },
            { "jumpfrom", "0"  },
            { "reply_mid", "0"  },
            { "reply_attr", "0"  },
            { "replay_dmid", ""  },
            { "statistics", "{\"appId\":100,\"platform\":5}"  },
            { "reply_type", "0"  },
            { "reply_uname", ""  },
            { "data_extend", "{\"trackid\":\"-99998\"}"  },
            { "fontsize", "25"  },
            { "rnd", ((int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds())).ToString()  },
            { "roomid", _biliService.RoomId.ToString()  },
            { "csrf", GetCsrfFromCookie() ?? ""  },
            { "csrf_token", GetCsrfFromCookie() ?? ""  }
        };
        
        var response = await _httpClient.PostAsync(new Uri(" https://api.live.bilibili.com/msg/send"),new FormUrlEncodedContent(formData));
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(responseStream);
        var code = jsonDoc.RootElement.GetProperty("code").GetInt32();
        if (code != 0)
        {
            var errMsg = jsonDoc.RootElement.GetProperty("message").GetString();
            throw new Exception(errMsg);
        }
    }
    
    public async Task SendGiftAsync()
    { 
        if (_biliService == null) throw new Exception("BiliService未初始化");
        var formData = new Dictionary<string, string>
        {
            { "uid", (await GetSelfUidAsyncAsync()).ToString()  }, //自己的uid
            { "gift_id", "31039" }, //礼物Id , 31039为牛蛙牛蛙
            { "ruid", (await GetTargetUidAsync(_biliService.RoomId.ToString())).ToString()  }, //主播的uid
            { "send_ruid", "0"  },
            { "gift_num", "1"  },
            { "coin_type", "gold"  },
            { "bag_id", "0"  },
            { "platform", "pc"  },
            { "biz_code", "Live"  },
            { "biz_id", _biliService.RoomId.ToString() }, //RoomId
            { "storm_beat_id", "0"  },
            { "metadata", ""  },
            { "price", "100"  },
            { "receive_users", ""  },
            { "live_statistics", "{\"pc_client\":\"pcWeb\",\"jumpfrom\":\"-99998\",\"room_category\":\"0\",\"source_event\":0,\"trackid\":\"-99998\",\"official_channel\":{\"program_room_id\":\"-99998\",\"program_up_id\":\"-99998\"}}"  },
            { "statistics", "{\"platform\":5,\"pc_client\":\"pcWeb\",\"appId\":100}"  },
            { "web_location", "444.8"  },
            { "csrf", GetCsrfFromCookie() ?? ""  },
            { "csrf_token", GetCsrfFromCookie() ?? ""  },
            { "wts", ((int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds())).ToString()  }
        };
        
        //wbi签名
        var query = await SignService.GetWebSignAsync(formData);
        var response = await _httpClient.PostAsync(new Uri(" https://api.live.bilibili.com/xlive/revenue/v1/gift/sendGold"),new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded"));
       
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(responseStream);
        var code = jsonDoc.RootElement.GetProperty("code").GetInt32();
        if (code != 0)
        {
            var errMsg = jsonDoc.RootElement.GetProperty("message").GetString();
            throw new Exception(errMsg);
        }
    }
    
    
    
    
    //
    //依赖方法
    //
    
    private async Task<Int64> GetTargetUidAsync(string roomId)
    {
        var roomInfoApi = 
            $"https://api.live.bilibili.com/xlive/play-interface/widgetService/GetWidgetBannerList?csrf={GetCsrfFromCookie()}&page_source=1&platform=pc&position=0&position_flag=0&room_id={roomId}&web_location=444.8";
        
        await using var responseStream = await _httpClient.GetStreamAsync(roomInfoApi);
        using var jsonDoc = await JsonDocument.ParseAsync(responseStream);
        var userId = jsonDoc.RootElement.GetProperty("data").GetProperty("ruid").GetInt64();
        return userId;
    }
    
    private async Task<Int64> GetSelfUidAsyncAsync()
    {
        var responseSting = await _httpClient.GetAsync(new Uri("https://api.bilibili.com/x/web-interface/nav"));
        var responseContent = await responseSting.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseContent);
        var userId = jsonDoc.RootElement.GetProperty("data").GetProperty("mid").GetInt64();
        return userId;
    }
    
    private string? GetCsrfFromCookie()
    {
        var cookies = _cookieContainer.GetCookies(new Uri("https://space.bilibili.com"));
        var cookie = cookies["bili_jct"];
        return cookie?.Value;
    }
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
    }
}