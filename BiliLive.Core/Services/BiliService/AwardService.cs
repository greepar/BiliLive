using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using BiliLive.Core.Models.BiliService;

namespace BiliLive.Core.Services.BiliService;

public class AwardService(HttpClient httpClient, CookieContainer cookieContainer)
{
    private const string RewardInfoUrl = "https://api.bilibili.com/x/activity_components/mission/info";
    private const string ReceiveAwardUrl = "https://api.bilibili.com/x/activity_components/mission/receive";

    public async Task<AwardInfo> GetAwardInfoAsync(string taskId)
    {
        var parameters = new Dictionary<string, string>
        {
            { "task_id", taskId },
            { "web_location", "888.126558" },
        };
        var query = await SignService.GetWebSignAsync(parameters);
        await using var response = await httpClient.GetStreamAsync($"{RewardInfoUrl}?{query}");
        using var jsonDoc = await JsonDocument.ParseAsync(response);
        var code = jsonDoc.RootElement.TryGetProperty("code", out var codeProp) ? codeProp.GetInt32() : -1;
        var message = jsonDoc.RootElement.TryGetProperty("message", out var msgProp)
            ? msgProp.GetString() ?? string.Empty
            : string.Empty;

        if (code != 0)
            throw new ApplicationException($"获取奖励信息出错，B站返回错误代码：{code}, 信息：{message}");

        if (!jsonDoc.RootElement.TryGetProperty("data", out var dataProp))
            throw new ApplicationException($"返回数据缺失，B站返回错误代码：{code}, 信息：{message}");

        var actId = dataProp.TryGetProperty("act_id", out var actIdProp) ? actIdProp.GetString() : null;
        var actName = dataProp.TryGetProperty("act_name", out var actNameProp) ? actNameProp.GetString() : null;
        var taskName = dataProp.TryGetProperty("task_name", out var taskNameProp) ? taskNameProp.GetString() : null;

        string? awardName = null;
        if (dataProp.TryGetProperty("reward_info", out var rewardProp))
        {
            awardName = rewardProp.TryGetProperty("award_name", out var awardNameProp)
                ? awardNameProp.GetString()
                : null;
        }

        if (actId == null || actName == null || taskName == null || awardName == null)
            throw new ApplicationException($"返回字段不完整，B站返回错误代码：{code}, 信息：{message}");

        return new AwardInfo(actId, actName, taskName, awardName);
    }

    public async Task<string> ClaimAwardAsync(string taskId)
    {
        //获取奖励信息
        var awardInfo = await GetAwardInfoAsync(taskId);
        //请求获取奖励
        var csrfValue = cookieContainer.GetCookies(new Uri("https://www.bilibili.com/"))["bili_jct"]?.Value ??
                        string.Empty;
        var formData = new Dictionary<string, string>
        {
            { "task_id", taskId },
            { "activity_id", awardInfo.ActId },
            { "activity_name", awardInfo.ActName },
            { "task_name", awardInfo.TaskName },
            { "reward_name", awardInfo.AwardName },
            { "gaia_vtoken", string.Empty },
            { "receive_from", "missionPage" },
            { "csrf", csrfValue },
        };

        
        var threadCount = 2; 
        var maxRetry = 120; 

        var cts = new CancellationTokenSource();

        var tasks = Enumerable.Range(0, threadCount)
            .Select(_ => TryReceiveAsync(cts.Token))
            .ToList();
        
        // 谁先完成
        var finishedTask = await Task.WhenAny(tasks);

        // 取消其他线程
        await cts.CancelAsync();
        return await finishedTask ?? throw new ApplicationException("领取奖励失败，所有线程均未成功");

        async Task<string> TryReceiveAsync(CancellationToken token)
        {
            var retry = 0;

            while (retry < maxRetry && !token.IsCancellationRequested)
            {
                try
                {
                    var receiveAwardQuery = await SignService.GetWebSignAsync();
                    using var response =
                        await httpClient.PostAsync($"{ReceiveAwardUrl}?{receiveAwardQuery}",
                            new FormUrlEncodedContent(formData), token);
                    await using var stream = await response.Content.ReadAsStreamAsync(token);
                    using var jsonDoc = await JsonDocument.ParseAsync(stream, cancellationToken: token);
                    var code = jsonDoc.RootElement.GetProperty("code").GetInt32();
                    Console.WriteLine(code);
                    switch (code)
                    {
                        case 0:
                            return jsonDoc.RootElement
                                .GetProperty("data")
                                .GetProperty("extra_info")
                                .GetProperty("cdkey_content")
                                .GetString() ?? throw new ApplicationException("领取奖励失败，B站返回数据缺失");
                    }
                }
                catch (HttpRequestException)
                {
                    // 网络异常重试
                }
                catch (TaskCanceledException)
                {
                    // 超时重试
                }
                retry++;
                await Task.Delay(1000, token);
            }

            throw new ApplicationException("领取奖励失败，超过最大重试次数");
            //未来也许会上验证码检测，届时再处理...
        }
    }
}
