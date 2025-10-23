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
    public enum ChangeType
    {
        Title,
        Area,
        Cover
    }

    private const string StartLiveUrl = "https://api.live.bilibili.com/room/v1/Room/startLive";
    private const string StopLiveUrl = "https://api.live.bilibili.com/room/v1/Room/stopLive";
    
    private const string GetMyChooseAreaUrl =
        "https://api.live.bilibili.com/room/v1/Area/getMyChooseArea?roomid=";
    private const string RoomInfoUrl =
        "https://api.live.bilibili.com/xlive/app-blink/v1/preLive/PreLive?platform=web&mobi_app=web&build=1";

    private const string RoomIdUrl = "https://api.live.bilibili.com/xlive/app-blink/v1/highlight/getRoomHighlightState";
    private const string AreasInfoUrl = "https://api.live.bilibili.com/xlive/app-blink/v1/preLive/GetAreaListForLive";
    private const string UpdateInfoUrl = "https://api.live.bilibili.com/room/v1/Room/update";

    private const string UpdatePreLiveInfoUrl = "https://api.live.bilibili.com/xlive/app-blink/v1/preLive/UpdatePreLiveInfo";
    private const string TodayRoomInfoUrl = "https://api.live.bilibili.com/xlive/anchor-task-interface/api/v1/CoreData?platform=web&mobi_app=web&build=1&range_type=1";


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
            RoomId = await GetRoomIdAsync(),
        };
    }

    public async Task<JsonElement> GetMyLastChooseAreaAsync()
    {
        var roomId = await GetRoomIdAsync();
        using var response = await httpClient.GetAsync(GetMyChooseAreaUrl + roomId);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var lastChooseArea = jsonDoc.RootElement.GetProperty("data").EnumerateArray().FirstOrDefault();
        return lastChooseArea.Clone();
    }
    
    public async Task<JsonElement> GetAreasListAsync()
    {
        using var response = await httpClient.GetAsync(AreasInfoUrl);
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        return jsonDoc.RootElement.Clone();
    }

    public async Task<JsonElement> StartLiveAsync()
    {
        var csrfValue = GetCsrfFromCookie();
        var lastAreaId = (await GetMyLastChooseAreaAsync()).GetProperty("id").GetString();
        var roomId = await GetRoomIdAsync();
        var formData = new Dictionary<string, string>
        {
            { "access_key", "" },
            { "area_v2", lastAreaId ?? throw new InvalidOperationException() },
            { "build", "9579" },
            { "csrf", csrfValue ?? "" },
            { "csrf_token", csrfValue ?? "" },
            { "platform", "pc_link" },
            { "room_id", roomId.ToString() },
            { "type", "2" },
            { "version", "7.23.0.9579" },
        };
        await SignService.AddAppSignAsync(formData);
        using var response = await httpClient.PostAsync(StartLiveUrl, new FormUrlEncodedContent(formData));
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var element = jsonDoc.RootElement;
        return element.Clone();
    }

    public async Task StopLiveAsync()
    {
        var response = await httpClient.PostAsync(StopLiveUrl, new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "room_id", (await GetRoomIdAsync()).ToString() },
            { "csrf", GetCsrfFromCookie() ?? "" },
            { "platform", "pc_link" },
        }));
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var root = jsonDoc.RootElement;
        if (root.GetProperty("code").GetInt32() != 0)
        {
            var msg = root.TryGetProperty("message", out var messageProp)
                ? messageProp.GetString()
                : "未知错误";
            throw new Exception($"服务器返回错误: {msg}");
        }
    }

    public async Task ChangeRoomInfoAsync(ChangeType type, object value)
    {
        var targetUrl = type != ChangeType.Cover ? UpdateInfoUrl : UpdatePreLiveInfoUrl;
        var roomId = await GetRoomIdAsync();
        var formData = new Dictionary<string, string>
        {
            { "csrf", GetCsrfFromCookie() ?? "" },
            { "csrf_token", GetCsrfFromCookie() ?? "" },
            { "room_id", roomId.ToString() },
            { "platform", "pc" },
            { "build", "1" },
            { "mobi_app", "web" }
        };

        switch (type)
        {
            case ChangeType.Title:
                if (value is string title && !string.IsNullOrWhiteSpace(title)) formData.Add("title", title);
                break;
            case ChangeType.Area:
                if (value is int areaId and > 0)
                {
                    formData.Add("area_id", areaId.ToString());
                    formData.Add("visit_id", "");
                }
                    
                else
                    throw new ArgumentException("分区ID无效", nameof(value));
                break;
            case ChangeType.Cover:
                if (value is string coverUrl && !string.IsNullOrWhiteSpace(coverUrl))
                {
                    throw new NotImplementedException();
                    // var imageUrl = await GetImageUrlAsync(cover);
                    //TODO: 实现封面上传
                    formData.Add("cover", coverUrl);
                }

                throw new ArgumentException("链接无效", nameof(value));
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        try
        {
            using var response = await httpClient.PostAsync(targetUrl, new FormUrlEncodedContent(formData));
            await using var stream = await response.Content.ReadAsStreamAsync();
            using var jsonDoc = await JsonDocument.ParseAsync(stream);
            var root = jsonDoc.RootElement;
            if (root.GetProperty("code").GetInt32() != 0)
            {
                var msg = root.TryGetProperty("message", out var messageProp)
                    ? messageProp.GetString()
                    : "未知错误";
                throw new Exception($"服务器返回错误: {msg}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"修改失败: {ex.Message}", ex);
        }
    }

    public async Task<JsonElement> GetLiveDataAsync(string liveKey)
    {
        await using var stream =
            await httpClient.GetStreamAsync(
                $"https://api.live.bilibili.com/xlive/app-blink/v1/live/StopLiveData?live_key={liveKey}");
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var element = jsonDoc.RootElement;
        return element.Clone();
    }
    public async Task<int> GetTodayLiveTimeAsync()
    {
        await using var stream = await httpClient.GetStreamAsync(TodayRoomInfoUrl);
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var element = jsonDoc.RootElement;
        //返回的为秒数
        var seconds = element
            .GetProperty("data")
            .GetProperty("list")
            .EnumerateArray()
            .Where(item => item.GetProperty("name").GetString() == "broadcast")
            .Select(item => item.GetProperty("value").GetInt32())
            .FirstOrDefault();
        return seconds;
    }

//私有依赖
    private async Task<string> GetImageUrlAsync(byte[] imageBytes)
    {
        //TODO: 实现封面上传
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

    private async Task GetSignAsync(Dictionary<string, string> parameters)
    {
        parameters.Add("ts", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
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

        var stringToSign = stringToSignBuilder.ToString();
        using var response =
            await httpClient.PostAsync("https://api.greepar.uk/getSign", new StringContent(stringToSign));
        var sign = await response.Content.ReadAsStringAsync();
        parameters.Add("sign", sign);
    }
}