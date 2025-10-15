using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BiliLive.Core.Services.BiliService;

public static class SignService
{
    public static async Task<string> GetAppSignAsync(string content)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.PostAsync("https://api.greepar.uk/getAppSign", new StringContent(content));
        using var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var sign = await response.Content.ReadAsStringAsync();
        return sign;
    }
    
    public static async Task<string> GetWebSignAsync(string query)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.PostAsync("https://api.greepar.uk/getWbiSign", new StringContent(query));
        using var jsonDoc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var signedQuery = await response.Content.ReadAsStringAsync();
        return signedQuery;
    }
}