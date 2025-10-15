using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BiliLive.Core.Services.BiliService;

public class AwardService(HttpClient httpClient)
{
    private async Task CliamAwardAsync(string rewardUrl)
    {
        const string rewardInfoApi = "https://api.bilibili.com/x/activity_components/mission/info";
            // ?task_id=6ERAzwloghvmcd00&web_location=888.126558&w_rid=fb5fad000e3e5cbdf6cee12a41e56213&wts=1760497483
        
        await using var infoResponse = await httpClient.GetStreamAsync(rewardInfoApi + rewardUrl);
        using var infoJsonDoc = await JsonDocument.ParseAsync(infoResponse);
        var actId = infoJsonDoc.RootElement.GetProperty("data").GetProperty("act_id").GetString();
        var actName = infoJsonDoc.RootElement.GetProperty("data").GetProperty("act_name").GetString();
        var taskId = infoJsonDoc.RootElement.GetProperty("data").GetProperty("task_id").GetString();
        var taskName = infoJsonDoc.RootElement.GetProperty("data").GetProperty("task_name").GetString();
        var awardName = infoJsonDoc.RootElement.GetProperty("data").GetProperty("award_name").GetString();
        
        var formData = new Dictionary<string,string>
        {
            {"task_id", taskId ?? string.Empty},
            {"activity_id", actId ?? string.Empty},
            {"activity_name", actName ?? string.Empty},
            {"task_name", taskName ?? string.Empty},
            {"reward_name", awardName ?? string.Empty},
            {"gaia_vtoken",string.Empty},
            {"receive_from", "missionPage"},
            {"csrf", string.Empty},
        };
        var content = new FormUrlEncodedContent(formData);
        var response = await httpClient.PostAsync("https://api.bilibili.com/x/activity_components/mission/receive?w_rid=9d5f3a193d29d7c09b8aa156f3a44edc&wts=1760497483\n", content);
    }
}