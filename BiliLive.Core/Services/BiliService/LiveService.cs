using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BiliLive.Core.Models.BiliService;

namespace BiliLive.Core.Services.BiliService;

internal class LiveService(HttpClient httpClient, CookieContainer cookieContainer)
{
    private const string StartLiveUrl = "https://api.live.bilibili.com/room/v1/Room/startLive";
    private const string RoomInfoUrl = "https://api.live.bilibili.com/xlive/app-blink/v1/preLive/PreLive?platform=web&mobi_app=web&build=1";
    private const string RoomIdUrl = "https://api.live.bilibili.com/xlive/app-blink/v1/highlight/getRoomHighlightState";

    public async Task<LiveRoomInfo> GetRoomInfoAsync()
    {
        var response = await httpClient.GetAsync(RoomInfoUrl);
        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseString);

        var roomCoverUrl = jsonDoc.RootElement.GetProperty("data").GetProperty("cover").GetProperty("url").GetString();
        var rcBytes = await httpClient.GetByteArrayAsync(roomCoverUrl);
        return new LiveRoomInfo
        {
            RoomCover = rcBytes,
            Title = jsonDoc.RootElement.GetProperty("data").GetProperty("title").GetString() ?? "未命名直播间",
            RoomId = 1.ToString()
        };
    }

    public async Task<string?> StartLiveAsync()
    {
        var csrfValue = GetCsrfFromCookie();
        var roomId = await GetRoomIdAsync();
        var formData = new Dictionary<string, string>
        {
            { "room_id", roomId ?? "1" },
            { "csrf", csrfValue ?? "" },
            { "platform", "pc_link" },
            { "area_v2", "321" }
            // { "csrf_token", csrfValue ?? "" },
            // { "type", "2"  },
            // { "build", "9579"  },
            // { "version", "7.23.0.9579"  },
            // { "appkey", "aae92bc66f3edfab"  },
            // { "access_key", ""  },
            // { "ts", ((Int32)(DateTimeOffset.UtcNow.ToUnixTimeSeconds())).ToString()  },
            // { "sign", ""  }
        };

        var response = await httpClient.PostAsync(StartLiveUrl, new FormUrlEncodedContent(formData));
        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseString);
        var responseCode = jsonDoc.RootElement.GetProperty("code").GetInt32();
        if (responseCode == 60024) return "错误:当前账号在触发风控，无法开播，尝试手机开播一次后再使用本软件开播";
        var apiKey = jsonDoc.RootElement.GetProperty("data").GetProperty("rtmp").GetProperty("code").GetString();
        return apiKey;
    }

    private async Task<string?> GetRoomIdAsync()
    {
        var response = await httpClient.GetAsync(RoomIdUrl);
        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseString);
        var roomId = jsonDoc.RootElement.GetProperty("data").GetProperty("room_id").GetInt64().ToString();
        return roomId;
    }

    private string? GetCsrfFromCookie()
    {
        var cookies = cookieContainer.GetCookies(new Uri("https://space.bilibili.com"));
        var cookie = cookies["bili_jct"];
        return cookie?.Value;
    }
}