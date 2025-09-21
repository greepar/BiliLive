using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace BiliLive.Core.Services;

public class GiftService : IDisposable
{
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:137.0) Gecko/20100101 Firefox/137.0";

    private readonly HttpClient _httpClient;
    private readonly CookieContainer _cookieContainer= new ();
    private bool _disposed;
    
    private readonly string _roomId;
    
    public GiftService(string roomId,string biliCookie,string? proxyAddress = null,string? username = null,string? password = null)
    {
        //传入RoomId
        _roomId = roomId;
        //添加Cookie
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
        
        if (!string.IsNullOrWhiteSpace(proxyAddress))
        {
            var proxy = new WebProxy(proxyAddress);
            if (!string.IsNullOrWhiteSpace(username))
            {
                proxy.Credentials = new NetworkCredential(username, password);
            }
            handler.UseProxy = true;
            handler.Proxy = proxy;
        }

        _httpClient = new HttpClient(handler, disposeHandler: true);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
    }
    
    //测试用，获取当前IP
    // public async Task GetCurrentIp()
    // {
    //     var aAResponseSting = await _httpClient.GetAsync(new Uri("https://4.ipw.cn"));
    //     var rResponseContent = await aAResponseSting.Content.ReadAsStringAsync();
    //     Console.WriteLine(rResponseContent);
    // }
    
    
    public async Task<bool> SendDanmakuAsync(String message)
    { 
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
            { "roomid", _roomId  },
            { "csrf", GetCsrfFromCookie() ?? ""  },
            { "csrf_token", GetCsrfFromCookie() ?? ""  }
        };
        
        var response = await _httpClient.PostAsync(new Uri(" https://api.live.bilibili.com/msg/send"),new FormUrlEncodedContent(formData));
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(responseStream);
        var code = jsonDoc.RootElement.GetProperty("code").GetInt32();
        return code == 0;
    }
    
    public async Task<bool> SendGiftAsync()
    { 
        var formData = new Dictionary<string, string>
        {
            { "uid", (await GetSelfUidAsyncAsync()).ToString()  }, //自己的uid
            { "gift_id", "31039" }, //礼物Id , 31039为牛蛙牛蛙
            { "ruid", (await GetTargetUidAsync(_roomId)).ToString()  }, //主播的uid
            { "send_ruid", "0"  },
            { "gift_num", "1"  },
            { "coin_type", "gold"  },
            { "bag_id", "0"  },
            { "platform", "pc"  },
            { "biz_code", "Live"  },
            { "biz_id", _roomId }, //RoomId
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
        var (imgKey, subKey) = await GetWbiKeys();
        Dictionary<string, string> signedParams = EncWbi(
            parameters: formData,
            imgKey: imgKey,
            subKey: subKey
        );
        string query = await new FormUrlEncodedContent(signedParams).ReadAsStringAsync();
        var response = await _httpClient.PostAsync(new Uri(" https://api.live.bilibili.com/xlive/revenue/v1/gift/sendGold"),new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded"));
       
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(responseStream);
        var code = jsonDoc.RootElement.GetProperty("code").GetInt32();
        if (code == 0)
        {
            return true;
        }
        var errMsg = jsonDoc.RootElement.GetProperty("message").GetString();
        throw new Exception(errMsg);
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
    
    // 算法
    private static readonly int[] MixinKeyEncTab =
    [
        46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35, 27, 43, 5, 49, 33, 9, 42, 19, 29, 28, 14, 39,
        12, 38, 41, 13, 37, 48, 7, 16, 24, 55, 40, 61, 26, 17, 0, 1, 60, 51, 30, 4, 22, 25, 54, 21, 56, 59, 6, 63,
        57, 62, 11, 36, 20, 34, 44, 52
    ];

    //对 imgKey 和 subKey 进行字符顺序打乱编码
    private static string GetMixinKey(string orig)
    {
        return MixinKeyEncTab.Aggregate("", (s, i) => s + orig[i])[..32];
    }

    private static Dictionary<string, string> EncWbi(Dictionary<string, string> parameters, string imgKey,
        string subKey)
    {
        string mixinKey = GetMixinKey(imgKey + subKey);
        string currTime = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        //添加 wts 字段
        parameters["wts"] = currTime;
        // 按照 key 重排参数
        parameters = parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        //过滤 value 中的 "!'()*" 字符
        parameters = parameters.ToDictionary(
            kvp => kvp.Key,
            kvp => new string(kvp.Value.Where(chr => !"!'()*".Contains(chr)).ToArray())
        );
        // 序列化参数
        string query = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;
        //计算 w_rid
        using MD5 md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query + mixinKey));
        string wbiSign = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        parameters["w_rid"] = wbiSign;

        return parameters;
    }

    // 获取最新的 img_key 和 sub_key
    private static async Task<(string, string)> GetWbiKeys()
      {
          var httpClient = new HttpClient();
          httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (HTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
          httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.bilibili.com/");
      
          HttpResponseMessage responseMessage = await httpClient.SendAsync(new HttpRequestMessage
          {
              Method = HttpMethod.Get,
              RequestUri = new Uri("https://api.bilibili.com/x/web-interface/nav"),
          });
      
          JsonNode response = JsonNode.Parse(await responseMessage.Content.ReadAsStringAsync())!;
      
          string imgUrl = (string)response["data"]!["wbi_img"]!["img_url"]!;
          imgUrl = imgUrl.Split("/")[^1].Split(".")[0];
      
          string subUrl = (string)response["data"]!["wbi_img"]!["sub_url"]!;
          subUrl = subUrl.Split("/")[^1].Split(".")[0];
          return (imgUrl, subUrl);
      }

      //算法
    
    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
    }
}