using System;
using System.Net.Http;
using System.Threading.Tasks;
using BiliLive.Core.Models.BiliService;

namespace BiliLive.Core.Services.BiliService;

public class LiveService(HttpClient httpClient)
{

    public async Task<LiveRoomInfo> GetRoomInfoAsync()
    {
        var response = await httpClient.GetAsync("https://api.live.bilibili.com/xlive/app-blink/v1/preLive/PreLive?platform=web&mobi_app=web&build=1");
        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDoc = System.Text.Json.JsonDocument.Parse(responseString);

        var roomCoverUrl = jsonDoc.RootElement.GetProperty("data").GetProperty("cover").GetProperty("url").GetString();
        var rcBytes = await httpClient.GetByteArrayAsync(roomCoverUrl) ;
        return new LiveRoomInfo()
        {
            RoomCover = rcBytes,
            Title = jsonDoc.RootElement.GetProperty("data").GetProperty("title").GetString() ?? "未命名直播间",
            RoomId = 1.ToString()
        };
    }
}