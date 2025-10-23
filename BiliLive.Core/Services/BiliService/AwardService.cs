using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BiliLive.Core.Services.BiliService;

public class AwardService(HttpClient httpClient,CookieContainer cookieContainer)
{
    private const string RewardInfoUrl = "https://api.bilibili.com/x/activity_components/mission/info";
    private const string ReceiveAwardUrl = "https://api.bilibili.com/x/activity_components/mission/receive";

    public async Task<string?> ClaimAwardAsync(string taskId)
    {
        var infoParameters = new Dictionary<string, string>
        {
            { "task_id", taskId },
            { "web_location", "888.126558" },
        };
        var query = await SignService.GetWebSignAsync(infoParameters);
        await using var infoResponse = await httpClient.GetStreamAsync($"{RewardInfoUrl}?{query}");
        using var infoJsonDoc = await JsonDocument.ParseAsync(infoResponse);
        var actId = infoJsonDoc.RootElement.GetProperty("data").GetProperty("act_id").GetString();
        var actName = infoJsonDoc.RootElement.GetProperty("data").GetProperty("act_name").GetString();
        var taskName = infoJsonDoc.RootElement.GetProperty("data").GetProperty("task_name").GetString();
        var awardName = infoJsonDoc.RootElement.GetProperty("data").GetProperty("reward_info").GetProperty("award_name")
            .GetString();

        var csrfValue = cookieContainer.GetCookies(new Uri("https://www.bilibili.com/"))["bili_jct"]?.Value ??
                        string.Empty;
        var formData = new Dictionary<string, string>
        {
            { "task_id", taskId },
            { "activity_id", actId ?? string.Empty },
            { "activity_name", actName ?? string.Empty },
            { "task_name", taskName ?? string.Empty },
            { "reward_name", awardName ?? string.Empty },
            { "gaia_vtoken", string.Empty },
            { "receive_from", "missionPage" },
            { "csrf", csrfValue },
        };
        var receiveQuery = await SignService.GetWebSignAsync();
        using var response =
            await httpClient.PostAsync($"{ReceiveAwardUrl}?{receiveQuery}", new FormUrlEncodedContent(formData));
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var jsonDoc = await JsonDocument.ParseAsync(stream);
        var code = jsonDoc.RootElement.GetProperty("code").GetInt32();
        var message = jsonDoc.RootElement.GetProperty("message").GetInt32();
        if (code == 0)
        {
            try
            {
                var cdKey = jsonDoc.RootElement.GetProperty("data").GetProperty("extra_info")
                    .GetProperty("cdkey_content").GetString();
                return cdKey;
            }
            catch (JsonException)
            {
                return null;
            }
        }
        throw new ApplicationException($"获取出错，B站返回错误代码：{code}, 信息：{message}");
    }
}
