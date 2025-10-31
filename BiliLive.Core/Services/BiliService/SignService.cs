using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BiliLive.Core.Services.BiliService;

public static class SignService
{
    private const string SignApi = "https://api.greepar.uk/getBiliSign/app";
    private const string SignUserAgent = "LiveHime/7.23.0.9579 os/Windows pc_app/livehime build/9579 osVer/10.0_x86_64";
    public static async Task AddAppSignAsync(Dictionary<string, string> parameters)
    {
        // 添加Sign所需参数
        parameters.Add("ts", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        parameters.Add("appkey", "aae92bc66f3edfab");
    
        var query = string.Join("&", parameters.Select(kv => 
            $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", SignUserAgent);
        using var response =
            await httpClient.PostAsync(SignApi, new StringContent(query));
        var sign = await response.Content.ReadAsStringAsync();
        parameters.Add("sign", sign);
    }
    
    public static async Task<string> GetWebSignAsync(Dictionary<string, string>? parameters = null)
    {
        parameters ??= new Dictionary<string, string>();
        //获取 imgKey 和 subKey
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", SignUserAgent);
        // httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (HTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
        httpClient.DefaultRequestHeaders.Referrer = new Uri("https://www.bilibili.com/");
        await using var response = await httpClient.GetStreamAsync(new Uri("https://api.bilibili.com/x/web-interface/nav"));
        using var jsonDoc = await JsonDocument.ParseAsync(response);
        
        var imgKey = jsonDoc.RootElement.GetProperty("data").GetProperty("wbi_img").GetProperty("img_url").GetString()?.Split("/")[^1].Split(".")[0];
        var subKey = jsonDoc.RootElement.GetProperty("data").GetProperty("wbi_img").GetProperty("sub_url").GetString()?.Split("/")[^1].Split(".")[0];
        if (imgKey == null || subKey == null) throw new JsonException("获取WBI密钥失败");
        
        // 算法
        int[] mixinKeyEncTab = [
            46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35, 27, 43, 5, 49, 33, 9, 42, 19, 29, 28, 14, 39,
            12, 38, 41, 13, 37, 48, 7, 16, 24, 55, 40, 61, 26, 17, 0, 1, 60, 51, 30, 4, 22, 25, 54, 21, 56, 59, 6, 63,
            57, 62, 11, 36, 20, 34, 44, 52
        ];
        
        //对 imgKey 和 subKey 进行字符顺序打乱编码
        var mixinKey = mixinKeyEncTab.Aggregate("", (s, i) => s + (imgKey + subKey)[i])[..32];
        //添加 wts 字段
        parameters["wts"] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
        // 按照 key 重排参数
        parameters = parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
        //过滤 value 中的 "!'()*" 字符
        parameters = parameters.ToDictionary(
            kvp => kvp.Key,
            kvp => new string(kvp.Value.Where(chr => !"!'()*".Contains(chr)).ToArray())
        );
        // 序列化参数
        string query = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;
        //计算 w_rid
        using MD5 md5 = MD5.Create();
        byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query + mixinKey));
        string wbiSign = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        parameters["w_rid"] = wbiSign;
        
        // 重新序列化参数
        var signedQuery = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();
        return signedQuery;
    }
}