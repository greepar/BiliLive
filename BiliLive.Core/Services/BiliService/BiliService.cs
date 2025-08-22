using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BiliLive.Core.Models.BiliService;

namespace BiliLive.Core.Services.BiliService;

public class BiliService
{
    private readonly HttpClient _httpClient;
    private const string UserAgent =
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:138.0) Gecko/20100101 Firefox/138.0";
    private readonly CookieContainer _cookieContainer;

    public BiliService(HttpClient httpClient, CookieContainer cookieContainer)
    {
        _cookieContainer = cookieContainer;
        _httpClient = httpClient;
        
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
    }

    public async Task<LoginResult> LoginAsync(string? biliCookie = null)
    {
        if (biliCookie == null)
        {
            var uri = new Uri("https://space.bilibili.com");
            var cookie = _cookieContainer.GetCookies(uri);
            biliCookie = string.Join(";", cookie.Select(c => $"{c.Name}={c.Value}"));
        }

        
            //已存在Cookie，直接检查Cookie是否有效
            var cookiePairs = biliCookie.Split(';');

            foreach (var pair in cookiePairs)
            {
                var cookieParts = pair.Split('=', 2);
                if (cookieParts.Length != 2) continue;

                var name = cookieParts[0].Trim();
                var value = cookieParts[1].Trim();

                // 添加 Cookie，Domain 设置为 .bilibili.com，让所有子域名共享
                _cookieContainer.Add(new Cookie(name, value)
                {
                    Domain = ".bilibili.com",
                    Path = "/"
                });
            }
            
            var checkLoginApi = "https://api.bilibili.com/x/web-interface/nav";
            var response = await _httpClient.GetAsync(checkLoginApi);
        
            using var jsonDoc = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);
        
            
            if (response.IsSuccessStatusCode)
            {
                var isLogin = jsonDoc.RootElement.GetProperty("data").GetProperty("isLogin").GetBoolean();
                if (isLogin)
                {
                    var userFaceUrl = jsonDoc.RootElement.GetProperty("data").GetProperty("face").GetString() ?? "Unknown";
                    var userFaceBytes = await _httpClient.GetByteArrayAsync(userFaceUrl);
                    return new LoginSuccess()
                    {
                        BiliCookie = biliCookie,
                        UserName = jsonDoc.RootElement.GetProperty("data").GetProperty("uname").GetString() ?? "Unknown",
                        UserId = jsonDoc.RootElement.GetProperty("data").GetProperty("mid").GetInt64(),
                        UserFaceBytes = userFaceBytes
                    };
                }
                return new LoginFailed()
                {
                    ErrorMsg = "登录信息失效了，请重新扫码登录..."
                };
            }
        

        return new LoginFailed()
        {
            ErrorMsg = "登录信息失效了，请重新扫码登录..."
        };
    }
    
    
    public async Task<QrLoginInfo?> GetLoginUrlAsync()
    {
        var loginApi = "https://passport.bilibili.com/x/passport-login/web/qrcode/generate";
        var loginResponse = await _httpClient.GetAsync(loginApi);
        if (loginResponse.IsSuccessStatusCode)
        {
            var loginResponseString = await loginResponse.Content.ReadAsStringAsync();
            try
            {
                using var loginJsonDoc = JsonDocument.Parse(loginResponseString);
             
                var loginUrl = loginJsonDoc.RootElement.GetProperty("data").GetProperty("url").GetString() ?? throw new InvalidOperationException();
                    var qrCodeKey = loginJsonDoc.RootElement.GetProperty("data").GetProperty("qrcode_key").GetString() ?? throw new InvalidOperationException();
                    return new QrLoginInfo()
                    {
                        QrCodeUrl = loginUrl,
                        QrCodeKey = qrCodeKey 
                    };
                    
                    // var loginCheckApi =
                    //     $"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={qrCodeKey}";
                    // for (var i = 0; i < 1; i++)
                    // {
                    //     using var response = await _httpClient.GetAsync(loginCheckApi);
                    //     var stringResponse = await response.Content.ReadAsStringAsync();
                    //     using var jsonDoc = JsonDocument.Parse(stringResponse);
                    //     if (response.IsSuccessStatusCode)
                    //     {
                    //         try
                    //         {
                    //             var apiResultCode = jsonDoc.RootElement.GetProperty("data").GetProperty("code")
                    //                 .GetInt32();
                    //             if (apiResultCode == 0)
                    //             {
                    //                 Console.WriteLine("登录成功，正在载入Cookie...");
                    //                 var uri = new Uri("https://space.bilibili.com");
                    //                 var cookie = _cookieContainer.GetCookies(uri);
                    //                 _biliCookie = string.Join(";", cookie.Select(c => $"{c.Name}={c.Value}"));
                    //                 Console.WriteLine("Cookie载入成功: " + _biliCookie);
                    //                 // await ConfigManager.SaveConfigAsync(_biliCookie);
                    //                 return true;
                    //             }
                    //
                    //             if (apiResultCode == 86101)
                    //             {
                    //                 //未扫码，继续等待
                    //             }
                    //             else if (apiResultCode == 86090)
                    //             {
                    //                 //已扫码，等待手机确认登录
                    //             }
                    //             else if (apiResultCode == 86038)
                    //             {
                    //                 // await new ErrorInfo("登录二维码已失效，请重新获取二维码。").ShowDialog(mainWindow);
                    //                 return false;
                    //             }
                    //             else
                    //             {
                    //                 // await new ErrorInfo($"未知返回代码: {apiResultCode}").ShowDialog(mainWindow);
                    //                 return false;
                    //             }
                    //         }
                    //         catch (JsonException e)
                    //         {
                    //             // await new ErrorInfo("JSON 转换出错: " + e.Message).ShowDialog(mainWindow);
                    //             return false;
                    //         }
                    //     }
                    //     else
                    //     {
                    //         // await new ErrorInfo("登录检查API请求失败，状态码: " + response.StatusCode).ShowDialog(mainWindow);
                    //         return false;
                    //     }
                    //
                    //     Console.WriteLine($"正在检查登录状态... {i + 1}/30");
                    //     await Task.Delay(2000); // 每2秒检查一次
                    // }
            }
            catch (JsonException)
            {
                // await new ErrorInfo("JSON 转换出错: " + e.Message).ShowDialog(mainWindow);
                return null;
            }
        }
        // await new ErrorInfo("二维码API 请求失败，状态码: " + loginResponse.StatusCode).ShowDialog(mainWindow);
        return null;
    }

    public async Task<Int32?> GeQrStatusCodeAsync(string qrCodeKey)
    {
        var loginCheckApi = $"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={qrCodeKey}";
        using var response = await _httpClient.GetAsync(loginCheckApi);
        var stringResponse = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(stringResponse);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var apiResultCode = jsonDoc.RootElement.GetProperty("data").GetProperty("code")
                    .GetInt32();
                return apiResultCode;
                // if (apiResultCode == 0)
                // {
                //     Console.WriteLine("登录成功，正在载入Cookie...");
                //     var uri = new Uri("https://space.bilibili.com");
                //     var cookie = _cookieContainer.GetCookies(uri);
                //     _biliCookie = string.Join(";", cookie.Select(c => $"{c.Name}={c.Value}"));
                //     Console.WriteLine("Cookie载入成功: " + _biliCookie);
                // }
                // if (apiResultCode == 86101)
                // {
                //     //未扫码，继续等待
                // }
                // else if (apiResultCode == 86090)
                // {
                //     //已扫码，等待手机确认登录
                // }
                // else if (apiResultCode == 86038)
                // {
                //     return false;
                // }
                // else
                // {
                //     return false;
                // }
            }
            catch (JsonException)
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }
}
