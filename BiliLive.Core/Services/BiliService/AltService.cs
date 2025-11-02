using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BiliLive.Core.Services.BiliService;

internal class AltService(HttpClient httpClient,CookieContainer cookieContainer)
{
    public async Task SendDanmakuAsync(string message,string roomId)
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
            { "roomid", roomId  },
            { "csrf", GetCsrfFromCookie() ?? ""  },
            { "csrf_token", GetCsrfFromCookie() ?? ""  }
        };
        
        var response = await httpClient.PostAsync(new Uri(" https://api.live.bilibili.com/msg/send"),new FormUrlEncodedContent(formData));
        await using var responseStream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(responseStream);
        var code = jsonDoc.RootElement.GetProperty("code").GetInt32();
        if (code != 0)
        {
            var errMsg = jsonDoc.RootElement.GetProperty("message").GetString();
            throw new Exception(errMsg);
        }
    }
    
    public async Task SendGiftAsync(string roomId)
    { 
        var formData = new Dictionary<string, string>
        {
            { "uid", (await GetSelfUidAsyncAsync()).ToString()  }, //自己的uid
            { "gift_id", "31039" }, //礼物Id , 31039为牛蛙牛蛙
            { "ruid", (await GetTargetUidAsync(roomId)).ToString()  }, //主播的uid
            { "send_ruid", "0"  },
            { "gift_num", "1"  },
            { "coin_type", "gold"  },
            { "bag_id", "0"  },
            { "platform", "pc"  },
            { "biz_code", "Live"  },
            { "biz_id", roomId }, //RoomId
            { "storm_beat_id", "0"  },
            { "metadata", ""  },
            { "price", "100"  },
            { "receive_users", ""  },
            { "live_statistics", "{\"pc_client\":\"pcWeb\",\"jumpfrom\":\"-99998\",\"room_category\":\"0\",\"source_event\":0,\"trackid\":\"-99998\",\"official_channel\":{\"program_room_id\":\"-99998\",\"program_up_id\":\"-99998\"}}"  },
            { "statistics", "{\"platform\":5,\"pc_client\":\"pcWeb\",\"appId\":100}"  },
            { "web_location", "444.8"  },
            { "csrf", GetCsrfFromCookie() ?? ""  },
            { "csrf_token", GetCsrfFromCookie() ?? ""  },
            { "wts", ((int)DateTimeOffset.UtcNow.ToUnixTimeSeconds()).ToString()  }
        };
        
        //wbi签名
        var query = await SignService.GetWebSignAsync(formData);
        var response = await httpClient.PostAsync(new Uri(" https://api.live.bilibili.com/xlive/revenue/v1/gift/sendGold"),new StringContent(query, Encoding.UTF8, "application/x-www-form-urlencoded"));
       
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
    private async Task<long> GetTargetUidAsync(string roomId)
    {
        var roomInfoApi = 
            $"https://api.live.bilibili.com/xlive/play-interface/widgetService/GetWidgetBannerList?csrf={GetCsrfFromCookie()}&page_source=1&platform=pc&position=0&position_flag=0&room_id={roomId}&web_location=444.8";
        
        await using var responseStream = await httpClient.GetStreamAsync(roomInfoApi);
        using var jsonDoc = await JsonDocument.ParseAsync(responseStream);
        var userId = jsonDoc.RootElement.GetProperty("data").GetProperty("ruid").GetInt64();
        return userId;
    }
    private async Task<long> GetSelfUidAsyncAsync()
    {
        var responseSting = await httpClient.GetAsync(new Uri("https://api.bilibili.com/x/web-interface/nav"));
        var responseContent = await responseSting.Content.ReadAsStringAsync();
        var jsonDoc = JsonDocument.Parse(responseContent);
        var userId = jsonDoc.RootElement.GetProperty("data").GetProperty("mid").GetInt64();
        return userId;
    }
    private string? GetCsrfFromCookie()
    {
        var cookies = cookieContainer.GetCookies(new Uri("https://space.bilibili.com"));
        var cookie = cookies["bili_jct"];
        return cookie?.Value;
    }
}