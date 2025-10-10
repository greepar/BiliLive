using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BiliLive.Core.Models.BiliService;

namespace BiliLive.Core.Services.BiliService;

internal class LiveService(HttpClient httpClient, CookieContainer cookieContainer)
{
    private const string StartLiveUrl = "https://api.live.bilibili.com/room/v1/Room/startLive";
    private const string RoomInfoUrl = "https://api.live.bilibili.com/xlive/app-blink/v1/preLive/PreLive?platform=web&mobi_app=web&build=1";
    private const string RoomIdUrl = "https://api.live.bilibili.com/xlive/app-blink/v1/highlight/getRoomHighlightState";
    private const string AreasInfoUrl = "https://api.live.bilibili.com/xlive/app-blink/v1/preLive/GetAreaListForLive";
    private const string UpdateInfoUrl = "https://api.live.bilibili.com/room/v1/Room/update";
    private const string UpdatePreLiveInfoUrl = "https://api.live.bilibili.com/xlive/app-blink/v1/preLive/UpdatePreLiveInfo";
    
    

    public async Task<LiveRoomInfo> GetRoomInfoAsync()
    {
        using var response = await httpClient.GetAsync(RoomInfoUrl);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(stream);

        var roomCoverUrl = jsonDoc.RootElement.GetProperty("data").GetProperty("cover").GetProperty("url").GetString();
        var rcBytes = await httpClient.GetByteArrayAsync(roomCoverUrl);
        return new LiveRoomInfo
        {
            RoomCover = rcBytes,
            Title = jsonDoc.RootElement.GetProperty("data").GetProperty("title").GetString() ?? "未命名直播间",
            RoomId = await GetRoomIdAsync() 
        };
    }

    public async Task GetAreasListAsync()
    {
        using var response = await httpClient.GetAsync(AreasInfoUrl);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var v1Area = jsonDoc.RootElement.GetProperty("data").GetProperty("area_v1_info");
        foreach (var area in v1Area.EnumerateArray())
        {
            var areaId = area.GetProperty("id").GetInt32();
            var areaName = area.GetProperty("name").GetString() ?? "未知分区";
            Console.WriteLine($"一级分区: {areaId} - {areaName}");
            var v2Areas = area.GetProperty("area_v2_info");
            foreach (var subArea in v2Areas.EnumerateArray())
            {
                var subAreaId = subArea.GetProperty("id").GetInt32();
                var subAreaName = subArea.GetProperty("name").GetString() ?? "未知子分区";
                Console.WriteLine($"\t二级分区: {subAreaId} - {subAreaName}");
            }
        }
        
    }
    
    public async Task<JsonElement> StartLiveAsync()
    {
        var csrfValue = GetCsrfFromCookie();
        try
        {
            var roomId = await GetRoomIdAsync();
            var formData = new Dictionary<string, string>
            {
                { "access_key", ""  },
                { "area_v2", "321" },
                { "build", "9579"  },
                { "csrf", csrfValue ?? "" },
                { "csrf_token", csrfValue ?? "" },
                { "platform", "pc_link" },
                { "room_id", roomId.ToString() },
                { "type", "2"  },
                { "version", "7.23.0.9579"  },
                //版本特定key
                { "appkey", "aae92bc66f3edfab" },
            };
            await GetSignAsync(formData);
            using var response = await httpClient.PostAsync(StartLiveUrl, new FormUrlEncodedContent(formData));
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var jsonDoc = await JsonDocument.ParseAsync(stream);
            var element = jsonDoc.RootElement;
            return element.Clone();
        }
        catch (Exception ex)
        {
            throw new Exception("开播失败:" + ex.Message);
        }
    }

    public async Task<string?> StopLiveAsync()
    {
        await Task.Delay(1);
        return "test";
    }
    
    public async Task ChangeRoomInfoAsync(string type, object value)
    {
        var roomId = await GetRoomIdAsync();
        var formData = new Dictionary<string, string>()
        {
            { "csrf", GetCsrfFromCookie() ?? "" },
            { "csrf_token", GetCsrfFromCookie() ?? "" },
            { "room_id", roomId.ToString() },
            { "platform", "web" },
            { "build", "1" },
            { "mobi_app", "web" }
        };
        switch (type)
        {
            case "title":
                if (value is string title && !string.IsNullOrWhiteSpace(title))
                {
                    try
                    {
                        formData.Add("title", title);
                        using var response = await httpClient.PostAsync(UpdateInfoUrl,new FormUrlEncodedContent(formData));
                        await using var stream = await response.Content.ReadAsStreamAsync();
                        using var jsonDoc = await JsonDocument.ParseAsync(stream);
                        var element = jsonDoc.RootElement;
                        if (element.GetProperty("code").GetInt32() != 0) throw new Exception(element.ToString());
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("修改标题失败:" + ex.Message);
                    }
                }
                break;
        }
    }
    
    public async Task<JsonElement> GetLiveDataAsync(string liveKey)
    {
        await using var stream = await httpClient.GetStreamAsync($"https://api.live.bilibili.com/xlive/app-blink/v1/live/StopLiveData?live_key={liveKey}");
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var element = jsonDoc.RootElement;
        return element.Clone();
        // {"code":0,"message":"0","ttl":1,"data":{"LiveTime":543,"AddFans":0,"HamsterRmb":0,"NewFansClub":0,"DanmuNum":0,"MaxOnline":2,"WatchedCount":1}}
    }
    
    private async Task<string> GetImageUrlAsync(byte[] imageBytes)
    {
        await Task.Delay(1);
        return "https://i0.hdslb.com/bfs/live/cover/1234567890.jpg";
    }
    
    private async Task<long> GetRoomIdAsync()
    {
        await using var response = await httpClient.GetStreamAsync(RoomIdUrl);
        using var jsonDoc = await JsonDocument.ParseAsync(response);
        var roomId = jsonDoc.RootElement.GetProperty("data").GetProperty("room_id").GetInt64();
        return roomId;
    }

    private string? GetCsrfFromCookie()
    {
        var cookies = cookieContainer.GetCookies(new Uri("https://space.bilibili.com"));
        var cookie = cookies["bili_jct"];
        return cookie?.Value;
    }
    
    private async Task GetSignAsync(Dictionary<string,string> parameters)
    {
        await Task.Delay(1);
        string unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        parameters.Add("ts", unixTimestamp);
        
        //按照 key 的字母顺序排序
        var sortedKeys = parameters.Keys.OrderBy(k => k, StringComparer.Ordinal);

        //  拼接成一个长字符串
        var stringToSignBuilder = new StringBuilder();
        foreach (var key in sortedKeys)
        {
            stringToSignBuilder.Append(key);
            stringToSignBuilder.Append("=");
            stringToSignBuilder.Append(parameters[key]); // 注意：value不需要URL编码
            stringToSignBuilder.Append("&");
        }
        
        // 移除最后一个多余的 '&'
        if (stringToSignBuilder.Length > 0)
            stringToSignBuilder.Length--;
        
        string stringToSign = stringToSignBuilder.ToString();
        using var response = await httpClient.PostAsync("https://api.greepar.uk/getSign", new StringContent(stringToSign));
        var sign = await response.Content.ReadAsStringAsync();
        parameters.Add("sign", sign);
    }
}