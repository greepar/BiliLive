using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
                if (value is byte[] { Length: > 0 } coverBytes)
                {
                    try
                    {
                        var coverUrl = await GetImageUrlAsync(coverBytes);
                        if (coverUrl.EndsWith(".bin"))
                        {
                            throw new Exception("封面上传失败，可能是图片格式不正确，请使用jpg或png格式的图片");
                        } 
                        formData.Add("cover", coverUrl);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("封面上传失败: " + ex.Message, ex);
                    }
                }
                else
                    throw new ArgumentException("封面图片数据无效", nameof(value));
                break;
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
        if (imageBytes == null || imageBytes.Length == 0)
            throw new ArgumentException("Image data is empty.");

        using var content = new MultipartFormDataContent();

        // 文件内容
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

        // multipart 表单参数
        var csrf = GetCsrfFromCookie() ?? throw new InvalidOperationException("无法从Cookie中获取CSRF令牌");
        content.Add(new StringContent("openplatform"), "bucket");
        content.Add(new StringContent(csrf), "csrf");
        content.Add(fileContent, "file", "cover.jpg");
        
        // 请求头
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.bilibili.com/x/upload/web/image");
        request.Content = content;

        using var response = await httpClient.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"上传失败: {response.StatusCode} - {body}");

        using var doc = JsonDocument.Parse(body);
        if (doc.RootElement.TryGetProperty("data", out var data) &&
            data.TryGetProperty("location", out var locationElement))
        {
            var url = locationElement.GetString();
            if (url != null)
                return url.Replace("http://", "https://");
        }

        var message = doc.RootElement.TryGetProperty("message", out var msgEl)
            ? msgEl.GetString()
            : "未知错误";
        throw new Exception($"上传失败: {message}");
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
}