using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BiliLive.Core.Models.BiliService;

// namespace BiliLive.Core.Services.BiliService;

public class LoginService
{
    private const string UserAgent =
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10.15; rv:138.0) Gecko/20100101 Firefox/138.0";

    private readonly CookieContainer _cookieContainer;
    private readonly HttpClient _httpClient;
    private string? _biliCookie = null!;

    public LoginService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _cookieContainer = new CookieContainer();
        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = _cookieContainer
        };
        _httpClient = new HttpClient(handler);
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
    }

    public async Task<LoginResult> LoginAsync(string? inputCookie)
    {       
        _biliCookie = inputCookie;
        //如果没有登录就触发登录程序
        if (_biliCookie == null)
        {
            _biliCookie = await TryLoginByQrCodeAsync();
            // return new LoginResult()
            // {
            //     IsSuccess = false,
            // };
        }
        
        if (_biliCookie != null)
        {
            //已存在Cookie，直接检查Cookie是否有效
            _cookieContainer.SetCookies(new Uri("https://api.bilibili.com/"), _biliCookie);
            var cookiePairs = _biliCookie.Split(';');
            foreach (var pair in cookiePairs)
            {
                var cookieParts = pair.Split('=', 2);
                if (cookieParts.Length != 2) continue;
                var name = cookieParts[0].Trim();
                var value = cookieParts[1].Trim();
                // 添加Cookie到CookieContainer
                _cookieContainer.Add(new Uri("https://www.bilibili.com"),
                    new Cookie(name, value) { Domain = ".bilibili.com" });
            }
            var checkLoginApi = "https://api.bilibili.com/x/web-interface/nav";
            var response = await _httpClient.GetAsync(checkLoginApi);
            
            using var jsonDoc = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);
            
            if (response.IsSuccessStatusCode)
            {
                var isLogin = jsonDoc.RootElement.GetProperty("data").GetProperty("isLogin").GetBoolean();
                if (isLogin)
                {
                    return new LoginSuccess()
                    {
                        UserName = jsonDoc.RootElement.GetProperty("data").GetProperty("uname").GetString() ?? "Unknown",
                        UserId = jsonDoc.RootElement.GetProperty("data").GetProperty("mid").GetInt64(),
                        UserFaceUrl = jsonDoc.RootElement.GetProperty("data").GetProperty("face").GetString() ?? "https://www.bilibili.com/favicon.ico"
                    };
                }
                return new LoginFailed()
                {
                    ErrorMsg = "登录信息失效了，请重新扫码登录..."
                };
            }
        }
        
        return new LoginFailed()
        {
            ErrorMsg = "登录信息失效了，请重新扫码登录..."
        };
    }

    private async Task<string?> TryLoginByQrCodeAsync()
    {
        await Task.Delay(1);
        // var loginApi = "https://passport.bilibili.com/x/passport-login/web/qrcode/generate";
        // var loginResponse = await _httpClient.GetAsync(loginApi);
        // if (loginResponse.IsSuccessStatusCode)
        // {
        //     var loginResponseString = await loginResponse.Content.ReadAsStringAsync();
        //     try
        //     {
        //         using var loginJsonDoc = JsonDocument.Parse(loginResponseString);
        //         var loginApiResultCode = loginJsonDoc.RootElement.GetProperty("code").GetInt32();
        //         if (loginApiResultCode == 0)
        //         {
        //             var qrCodeKey = loginJsonDoc.RootElement.GetProperty("data").GetProperty("qrcode_key").GetString();
        //             var loginUrl = loginJsonDoc.RootElement.GetProperty("data").GetProperty("url").GetString();
        //
        //             //生成二维码
        //             if (loginUrl == null || qrCodeKey == null) { return false;} 
        //             using var qrGenerator = new QRCodeGenerator();
        //             using var qrCodeData = qrGenerator.CreateQrCode(loginUrl, QRCodeGenerator.ECCLevel.Q);
        //             using var qrCode = new PngByteQRCode(qrCodeData);
        //             var qrCodeImage = qrCode.GetGraphic(20);
        //             // await new QrCodeWindow(qrCodeImage).ShowDialog(mainWindow);
        //             
        //             var loginCheckApi =
        //                 $"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={qrCodeKey}";
        //             for (var i = 0; i < 1; i++)
        //             {
        //                 using var response = await _httpClient.GetAsync(loginCheckApi);
        //                 var stringResponse = await response.Content.ReadAsStringAsync();
        //                 using var jsonDoc = JsonDocument.Parse(stringResponse);
        //                 if (response.IsSuccessStatusCode)
        //                 {
        //                     try
        //                     {
        //                         var apiResultCode = jsonDoc.RootElement.GetProperty("data").GetProperty("code")
        //                             .GetInt32();
        //                         if (apiResultCode == 0)
        //                         {
        //                             Console.WriteLine("登录成功，正在载入Cookie...");
        //                             var uri = new Uri("https://space.bilibili.com");
        //                             var cookie = _cookieContainer.GetCookies(uri);
        //                             _biliCookie = string.Join(";", cookie.Select(c => $"{c.Name}={c.Value}"));
        //                             Console.WriteLine("Cookie载入成功: " + _biliCookie);
        //                             // await ConfigManager.SaveConfigAsync(_biliCookie);
        //                             return true;
        //                         }
        //
        //                         if (apiResultCode == 86101)
        //                         {
        //                             //未扫码，继续等待
        //                         }
        //                         else if (apiResultCode == 86090)
        //                         {
        //                             //已扫码，等待手机确认登录
        //                         }
        //                         else if (apiResultCode == 86038)
        //                         {
        //                             // await new ErrorInfo("登录二维码已失效，请重新获取二维码。").ShowDialog(mainWindow);
        //                             return false;
        //                         }
        //                         else
        //                         {
        //                             // await new ErrorInfo($"未知返回代码: {apiResultCode}").ShowDialog(mainWindow);
        //                             return false;
        //                         }
        //                     }
        //                     catch (JsonException e)
        //                     {
        //                         // await new ErrorInfo("JSON 转换出错: " + e.Message).ShowDialog(mainWindow);
        //                         return false;
        //                     }
        //                 }
        //                 else
        //                 {
        //                     // await new ErrorInfo("登录检查API请求失败，状态码: " + response.StatusCode).ShowDialog(mainWindow);
        //                     return false;
        //                 }
        //
        //                 Console.WriteLine($"正在检查登录状态... {i + 1}/30");
        //                 await Task.Delay(2000); // 每2秒检查一次
        //             }
        //         }
        //         // await new ErrorInfo("登录失败了,请确认扫码后再点击已登录！").ShowDialog(mainWindow);
        //         return false;
        //     }
        //     catch (JsonException e)
        //     {
        //         // await new ErrorInfo("JSON 转换出错: " + e.Message).ShowDialog(mainWindow);
        //         return false;
        //     }
        // }
        // // await new ErrorInfo("二维码API 请求失败，状态码: " + loginResponse.StatusCode).ShowDialog(mainWindow);
        // return false;
        return ":";
    }
}