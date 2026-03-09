using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Core.Interface;
using BiliLive.Core.Models.BiliService;
using BiliLive.Resources;
using BiliLive.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace BiliLive.Views.MainWindow.Pages.About;

public partial class AboutViewModel : ViewModelBase
{
    // [ObservableProperty]private object _currentView = new DialControl();

    private readonly IBiliService _biliService;
    [ObservableProperty] private Bitmap _developerAvatar;

    public AboutViewModel(IServiceProvider? serviceProvider = null)
    {
        _biliService = serviceProvider?.GetService(typeof(IBiliService)) as IBiliService ??
                       throw new ArgumentNullException(nameof(serviceProvider));
        var file = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/userPic.jpg"));
        DeveloperAvatar = PicHelper.ResizeStreamToBitmap(file, 120, 120) ?? new Bitmap(file);
    }


    //调试模式

    [RelayCommand]
    private void SetTopMostWindow(bool isTopMost)
    {
#if DEBUG
        AvaloniaUtils.SetTopMostWindow(isTopMost);
#endif
    }

    [RelayCommand]
    private void OpenCurrentFolder()
    {
        // Console.WriteLine();
#if DEBUG
        var currentPath = AppDomain.CurrentDomain.BaseDirectory;
        Process.Start(new ProcessStartInfo
        {
            FileName = currentPath,
            UseShellExecute = true
        });
#endif
    }

    [RelayCommand]
    private static async Task TestAsync()
    {
        await ShowWindowHelper.ShowQrCodeAsync("\"这是一个测试错误消息\",\"https://www.bilibili.com/\"");
    }

    [RelayCommand]
    private async Task GetLiveCookieAsync()
    {
        if (!OperatingSystem.IsWindows())
        {
            await ShowWindowHelper.ShowErrorAsync("此功能仅在Windows系统上可用");
            return;
        }

        try
        {
            var localAppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var targetFile = Path.Combine(localAppDataPath, "bililive", "User Data", "Global", "Secret Preference");
            if (!File.Exists(targetFile))
            {
                await ShowWindowHelper.ShowErrorAsync("获取直播Cookie失败：Secret Preference 文件不存在");
                return;
            }

            //读取 cookies
            var jsonText = await File.ReadAllTextAsync(targetFile);
            using var doc = JsonDocument.Parse(jsonText);

            var cookiesBase64 = doc.RootElement
                .GetProperty("accounts")
                .GetProperty("history")[0]
                .GetProperty("cookies")
                .GetString();

            if (string.IsNullOrWhiteSpace(cookiesBase64))
            {
                await ShowWindowHelper.ShowErrorAsync("获取直播Cookie失败：cookies 字段为空");
                return;
            }

            // base64 -> 二进制密文
            var cipher = Convert.FromBase64String(cookiesBase64);

            // TEA解密
            byte[] key =
            [
                0x65, 0x4B, 0x58, 0x6A, 0x8A, 0xB7, 0x7F, 0x8E,
                0x0D, 0xBF, 0x68, 0xA7, 0x60, 0xF6, 0xE7, 0x89
            ];

            if (cipher.Length % 8 != 0)
            {
                await ShowWindowHelper.ShowErrorAsync($"获取直播Cookie失败：密文长度非法({cipher.Length})");
                return;
            }

            static uint ToUInt32Le(byte[] b, int o)
            {
                return (uint)(b[o] | (b[o + 1] << 8) | (b[o + 2] << 16) | (b[o + 3] << 24));
            }

            static void FromUInt32Le(uint v, byte[] b, int o)
            {
                b[o] = (byte)(v & 0xFF);
                b[o + 1] = (byte)((v >> 8) & 0xFF);
                b[o + 2] = (byte)((v >> 16) & 0xFF);
                b[o + 3] = (byte)((v >> 24) & 0xFF);
            }

            var k0 = ToUInt32Le(key, 0);
            var k1 = ToUInt32Le(key, 4);
            var k2 = ToUInt32Le(key, 8);
            var k3 = ToUInt32Le(key, 12);

            var plain = new byte[cipher.Length];
            for (var i = 0; i < cipher.Length; i += 8)
            {
                var v0 = ToUInt32Le(cipher, i);
                var v1 = ToUInt32Le(cipher, i + 4);
                var sum = 0xC6EF3720;

                for (var r = 0; r < 32; r++)
                {
                    v1 = unchecked(v1 - (((v0 >> 5) + k3) ^ ((v0 << 4) + k2) ^ (sum + v0)));
                    v0 = unchecked(v0 - (((v1 >> 5) + k1) ^ ((v1 << 4) + k0) ^ (sum + v1)));
                    sum = unchecked(sum + 0x61C88647);
                }

                FromUInt32Le(v0, plain, i);
                FromUInt32Le(v1, plain, i + 4);
            }

            // 去掉末尾 \0，得到明文JSON
            var end = plain.Length;
            while (end > 0 && plain[end - 1] == 0) end--;
            var cookiesJson = Encoding.UTF8.GetString(plain, 0, end);

            if (!cookiesJson.Contains("SESSDATA", StringComparison.Ordinal))
            {
                await ShowWindowHelper.ShowErrorAsync("解密完成，但未检测到 SESSDATA，可能账号未登录");
                return;
            }

            using var cookieDoc = JsonDocument.Parse(cookiesJson);
            var cookieMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in cookieDoc.RootElement.EnumerateArray())
            {
                if (!item.TryGetProperty("name", out var n) || !item.TryGetProperty("value", out var v))
                    continue;

                var name = n.GetString();
                var value = v.GetString();
                if (!string.IsNullOrWhiteSpace(name) && value is not null)
                    cookieMap[name] = value;
            }

            var liveCookie = string.Join(";",
                cookieMap.Select(kv => $"{kv.Key}={kv.Value}"));


            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("直播Cookie获取成功,尝试登录中...",
                Geometry.Parse(MdIcons.Check)));

            var loginResult = await _biliService.LoginAsync(liveCookie);
            switch (loginResult)
            {
                case LoginFailed:
                    await ShowWindowHelper.ShowErrorAsync("Cookie登录失败，可能是Cookie无效或过期");
                    break;
                case LoginSuccess:
                    WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("Cookie登录成功！",
                        Geometry.Parse(MdIcons.Check)));
                    WeakReferenceMessenger.Default.Send(new LoginMessage(loginResult));
                    break;
            }
        }
        catch (Exception ex)
        {
            await ShowWindowHelper.ShowErrorAsync($"获取直播Cookie失败：{ex.Message}");
        }
    }
}