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
    
    public GiftService(string biliCookie,string? proxyAddress = null,string? username = null,string? password = null)
    {
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
            
            _httpClient = new HttpClient(handler, disposeHandler: true);
        }
        else
        {           
            _httpClient = new HttpClient(handler, disposeHandler: true);
        }
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);

    }
 
    
    public async Task SendDanmakuAsync()
    { 
        var responseStinga = await _httpClient.GetAsync(new Uri("https://4.ipw.cn"));
        var responseContentb = await responseStinga.Content.ReadAsStringAsync();
        Console.WriteLine(responseContentb);
        
        var formData = new Dictionary<string, string>
        {
            { "bubble", "0"  },
            { "msg", "主播您好，祝您直播愉快！" },
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
            { "roomid", "10431980"  },
            { "csrf", GetCsrfFromCookie() ?? ""  },
            { "csrf_token", GetCsrfFromCookie() ?? ""  }
        };
        
       
        var responseSting = await _httpClient.PostAsync(new Uri(" https://api.live.bilibili.com/msg/send"),new FormUrlEncodedContent(formData));
        var responseContent = await responseSting.Content.ReadAsStringAsync();
        Console.WriteLine(responseContent);
    }
    
    public async Task SendGiftAsync()
    { 
        var responseStinga = await _httpClient.GetAsync(new Uri("https://4.ipw.cn"));
        var responseContentb = await responseStinga.Content.ReadAsStringAsync();
        Console.WriteLine(responseContentb);
        
        var formData = new Dictionary<string, string>
        {
            { "uid", "3493081477286220"  },//自己的uid
            { "gift_id", "31039" },//礼物ID
            { "ruid", "196431435"  },//主播的uid
            { "send_ruid", "0"  },
            { "gift_num", "1"  },
            { "coin_type", "gold"  },
            { "bag_id", "0"  },
            { "platform", "pc"  },
            { "biz_code", "Live"  },
            { "biz_id", "10431980"  },
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
       
        var (imgKey, subKey) = await GetWbiKeys();
        Dictionary<string, string> signedParams = EncWbi(
            parameters: formData,
            imgKey: imgKey,
            subKey: subKey
        );

        string query = await new FormUrlEncodedContent(signedParams).ReadAsStringAsync();

        Console.WriteLine(query);
       
        var responseSting = await _httpClient.PostAsync(new Uri(" https://api.live.bilibili.com/xlive/revenue/v1/gift/sendGold"),new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded"));
        
        var responseContent = await responseSting.Content.ReadAsStringAsync();
        Console.WriteLine(responseContent);
        // var responseSting = await _httpClient.PostAsync(new Uri(" https://api.live.bilibili.com/msg/send"),new FormUrlEncodedContent(formData));
        // var responseContent = await responseSting.Content.ReadAsStringAsync();
        // Console.WriteLine(responseContent);
    }
    
    //test
    // public async Task GetAccInfoAsync()
    // {
    //     var checkLoginApi = "https://api.bilibili.com/x/web-interface/nav";
    //     var responseString = await _httpClient.GetStringAsync(checkLoginApi);
    //     Console.WriteLine(responseString);
    // }
    
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
          httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
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