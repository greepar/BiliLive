using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using BiliLive.Core.Interface;
using BiliLive.Core.Models.BiliService;
using BiliLive.Core.Services;
using BiliLive.Core.Services.BiliService;
using BiliLive.Models;
using BiliLive.Resources;
using BiliLive.Services;
using BiliLive.Utils;
using BiliLive.Views.DialogWindow;
using BiliLive.Views.MainWindow.Pages.AutoService.Components;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace BiliLive.Views.MainWindow.Pages.AutoService;

public partial class Alt : ObservableObject
{
  
    //先赋值
    [ObservableProperty] private string _userName;
    [ObservableProperty] private bool _isGiftSent;
    [ObservableProperty] private bool _isDanmakuSent;
 
    //后获取信息
    [ObservableProperty] private Bitmap? _userFace;
    [ObservableProperty] private long? _userId;
   
    
    private readonly Action<Alt> _removeCallback;
    public IRelayCommand AltSettingsCommand { get; }
    public IRelayCommand RemoveAltsCommand { get; }
    public IRelayCommand SendDanmakuCommand { get; }
    public IRelayCommand SendGiftCommand { get; }
    
    
    //公共服务
    private readonly AltService _altService;
    
    private async Task AddAltsAsync()
    {
        var loginResult= await _altService.LoginAsync();
        if (loginResult is LoginSuccess result)
        {
            UserId = result.UserId;
            UserName = result.UserName;
            using var ms = new MemoryStream(result.UserFaceBytes);
            UserFace = await Task.Run(() => Bitmap.DecodeToWidth(ms, 80));
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"登录成功，当前账号 {result.UserName}",Geometry.Parse(MdIcons.Check)));
        }
        else if (loginResult is LoginFailed failed)
        {
            var errorMsg = failed.IsCanceled ? "登录已取消" : $"登录失败 {failed.ErrorMsg}";
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(errorMsg,Geometry.Parse(MdIcons.Error)));
        }
    }
    
    public Alt(AltConfig altConfig, Action<Alt> removeCallback)
    {
        //初始化
        _removeCallback = removeCallback;
        UserName = altConfig.UserName;
        _altService = new AltService(altConfig.CookieString);
        // IsDanmakuSent = altConfig.;
        IsGiftSent = altConfig.IsSendGift;
        
        
        // UserId = userInfo.UserId;
      
        
        // var selfRoomId = (await _biliService.GetRoomInfoAsync()).RoomId;
        
        // using var giftService = new GiftService(selfRoomId,cookie,"http://142.111.48.253:7030","pvcufsir","5ppw7212petm");
        // var cookie = "CURRENT_QUALITY=0;b_lsid=3D9C828D_19966E7C879;theme-tip-show=SHOWED;home_feed_column=4;LIVE_BUVID=AUTO3917563974158518;buvid4=693757A4-F7AE-46F7-FD89-176D5864FF0465440-025080417-+Y68DEMsE7icBBnI4Ogsb8zMS4RLFD0sjE3sN1E7Y+p84Jp9uaGgje/3Dwd3EJRN;CURRENT_FNVAL=2000;buvid3=AAD37E52-3183-C9A7-8B21-A29BA301223E64690infoc;share_source_origin=COPY;sid=53fl154r;SESSDATA=921eeba8%2C1773900917%2C88c6b%2A91CjDngIQNz7HBaLpv599D40Uaey4Gl0GyJ09-mIbnwYoSaFDE_s3f1u5Es1naJ7zEBO4SVkZDdEFwdlJ2S1A2OHkzcEJoZUNPcm81cTlEem5WTWx6NTRLcm12QzJaRnpSRnlnR0VvWVJNRS15dWM5cjI4Q2Nnel9aUnNOdlhFWjFOVVBiZmlyZG13IIEC;bsource=share_source_copy_link;bmg_src_def_domain=i1.hdslb.com;theme-avatar-tip-show=SHOWED;b_nut=1754298164;_uuid=8F10F9DCF-3A93-1186-A31D-3E4D39CBACFB63157infoc;bili_jct=b262eb807ec125b7e6ac1c06c54aa26e;bili_ticket=eyJhbGciOiJIUzI1NiIsImtpZCI6InMwMyIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3NTg2MTYzOTMsImlhdCI6MTc1ODM1NzEzMywicGx0IjotMX0.axlnDqIVwLHxwg9wx1YZxgZP4fLVTLMefLVAf2IKf9I;bili_ticket_expires=1758616333;bmg_af_switch=1;bp_t_offset_3493081477286220=1114632377969147904;browser_resolution=1385-744;buvid_fp=4973860c9239ec9f36c712d0d6a5b2e3;DedeUserID=3493081477286220;DedeUserID__ckMd5=5625cdd7363cee29;enable_web_push=DISABLE;hit-dyn-v2=1;PVID=1;rpdid=|(u|u~J~mu)~0J'u~l)JR)mku";

        // await giftService.SendDanmakuAsync("您好，这是一条测试弹幕");
        // await giftService.SendGiftAsync();
        
        SendDanmakuCommand = new RelayCommand(async () =>
        {
            var dialogVm = new DialogWindowViewModel
            {
                Message = $"确认发送弹幕吗？",
            };
            await ShowWindowHelper.ShowWindowAsync(new DialogWindow.DialogWindow(){DataContext =dialogVm});
            if (dialogVm.IsConfirmed)
            {
                //待办 发送弹幕
                IsDanmakuSent = true;
                WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"已为账号 {UserName} 发送弹幕",Geometry.Parse(MdIcons.Check)));
            }
        });
        
        SendGiftCommand = new RelayCommand(async () =>
        {
            var dialogVm = new DialogWindowViewModel
            {
                Message = $"确认发送礼物吗？",
            };
            await ShowWindowHelper.ShowWindowAsync(new DialogWindow.DialogWindow(){DataContext =dialogVm});
            if (dialogVm.IsConfirmed)
            {
                //待办 发送礼物
                IsGiftSent = true;
                WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"已为账号 {UserName} 发送礼物",Geometry.Parse(MdIcons.Check)));
            }
        });
        
        AltSettingsCommand = new RelayCommand(async () =>
        {
            
            // using var tempMs = new MemoryStream(userInfo.UserFace);
            var altVm = new AltsManagerViewModel(_altService,true)
            {
                CookieValue = altConfig.CookieString,
                //AltManagerVM后台会自动Dispose
                // QrCodePic = new Bitmap(tempMs),
            };
            
            await ShowWindowHelper.ShowWindowAsync(new AltsManager(){DataContext = altVm});
            if (altVm.AllowDoneClose)
            {
                var cookie = altVm.CookieValue;
                Console.WriteLine(cookie);
            }
        });
        
        RemoveAltsCommand = new RelayCommand(async () =>
        {
            try
            {
                //清除当前账号
                var dialogVm = new DialogWindowViewModel
                {
                    Message = $"确认清除账号 {UserName} 吗？",
                };
                await ShowWindowHelper.ShowWindowAsync(new DialogWindow.DialogWindow(){DataContext =dialogVm});
                if (dialogVm.IsConfirmed)
                {
                    removeCallback(this);
                    //结束时释放服务
                    _altService.Dispose();
                    UserFace.Dispose();
                    WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("已清除当前账号",Geometry.Parse(MdIcons.Check)));
                }
            }
            catch (Exception)
            {
                // ignored
            }
        });
    }
}

public partial class AutoServiceViewModel : ViewModelBase
{
    [ObservableProperty]private ObservableCollection<Alt> _altsList = [];
    
    [ObservableProperty] private string? _ffmpegPath;
    [ObservableProperty] private string? _videoPath;
    
    [ObservableProperty] private bool _isEnabled;
    [ObservableProperty] private bool _isAutoStart;
    [ObservableProperty] private bool _isRandomSecond;
    [ObservableProperty] private bool _isCheck60MinTask;
    
    //时间
    [ObservableProperty] private string? _startHour;
    [ObservableProperty] private string? _startMinute;
    [ObservableProperty] private string? _startSecond;

    private readonly IBiliService _biliService;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedStartTime))]
    private TimeSpan _startTime = new(0, 5, 0);
    public string FormattedStartTime => StartTime.ToString(@"hh\:mm\:ss");

    
    public AutoServiceViewModel(IServiceProvider? serviceProvider = null)
    {

        using var faceMs = AssetLoader.Open(new Uri("avares://BiliLive/Assets/Pics/userPic.jpg"));
        
        using var ms = new MemoryStream();
        faceMs.CopyTo(ms);
        ms.Position = 0;
        
        var userinfo = new UserInfo
        {
            UserFace =(ms).ToArray(),
            UserName = "账号1",
            UserId = "ID2"
        };
        
        
        
        // AltsList.Add(new Alt("userinfo","哥瑞普",RemoveAlt,true,true));
        
        // AltsList.Add(new Alt(new UserInfo
        // {
        //     UserFace =(ms).ToArray(),
        //     UserName = "账号2",
        //     UserId = "ID3"
        // },2,RemoveAlt,true,false));
        
        if (Design.IsDesignMode)
        {
            //设计时数据
            _biliService = new BiliServiceImpl();
        }
        else
        {
            _biliService = serviceProvider!.GetRequiredService<IBiliService>();
        }
    }
    
    [RelayCommand]
    private async Task ToggleOptions()
    {
        var random = new Random();
        var hour   = int.TryParse(StartHour,   out var h) ? h : 0;
        var minute = int.TryParse(StartMinute, out var m) ? m : 0;
        var second = int.TryParse(StartSecond, out var s) ? s : 0;

        var seconds = hour * 3600 +
                      minute * 60 +
                      (IsRandomSecond ? random.Next(-240, -120) : second);
        
        
        //设置第二天基准
  
        var baseSeconds = StartTime.TotalSeconds;
        var finalSeconds = IsRandomSecond ? baseSeconds + random.Next(-240, -120) : baseSeconds;
        var streamTime = DateTime.Today.AddDays(1).AddSeconds(finalSeconds);
        
        // WeakReferenceMessenger.Default.Send(new ShowNotificationMessage($"设置时间{streamTime}",Geometry.Parse(MdIcons.Check)));
        
        
        var popupMsg = IsEnabled ? $"自动开播将在 {streamTime} 开始" : "已关闭自动开播服务";
        var icon = IsEnabled ? Geometry.Parse(MdIcons.Check) : Geometry.Parse(MdIcons.Error);
        WeakReferenceMessenger.Default.Send(new ShowNotificationMessage(popupMsg,icon));
        await ConfigManager.SaveConfigAsync(ConfigType.EnableAutoService,IsEnabled);
    }

    [RelayCommand]
    private async Task PickFfmpegPathAsync()
    {
        string[] defaultFileExtension = OperatingSystem.IsWindows() ? [".exe"] : [""];
        var pickFile = await FolderPickHelper.PickFileAsync("Choose Ffmpeg Path",defaultFileExtension);
        if (string.IsNullOrWhiteSpace(pickFile))
            return;
        if (await FfmpegWrapper.CheckFfmpegAvailableAsync(pickFile))
        {
            FfmpegPath = pickFile;
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("Ffmpeg路径有效",Geometry.Parse(MdIcons.Check)));
            await ConfigManager.SaveConfigAsync(ConfigType.FfmpegPath,FfmpegPath);
        }
        else
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("Ffmpeg路径无效，请重新选择",Geometry.Parse(MdIcons.Error)));
        }
    }
    
    [RelayCommand]
    private async Task PickVideoPathAsync()
    {
        if (string.IsNullOrWhiteSpace(FfmpegPath))
        {
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("请先设置Ffmpeg路径",Geometry.Parse(MdIcons.Notice)));
            return;
        }
        var pickFile = await FolderPickHelper.PickFileAsync("Choose Video Path",[".mp4",".flv",".mkv",".mov",".avi"]);
        if (string.IsNullOrWhiteSpace(pickFile))
            return;
        if (!await FfmpegWrapper.CheckVideoAvailableAsync(FfmpegPath,pickFile))
        { 
            WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("视频文件无效，请重新选择",Geometry.Parse(MdIcons.Error)));
            return;
        }
     
        VideoPath = pickFile;
        WeakReferenceMessenger.Default.Send(new ShowNotificationMessage("视频文件有效",Geometry.Parse(MdIcons.Check)));
        await ConfigManager.SaveConfigAsync(ConfigType.VideoPath,VideoPath);
    }
    
    [RelayCommand]
    private async Task AutoStartOptionAsync()
    {
        await ConfigManager.SaveConfigAsync(ConfigType.AutoStart,IsAutoStart);
    }
    
    [RelayCommand]
    private async Task Check60MinTaskAsync()
    {
        await ConfigManager.SaveConfigAsync(ConfigType.Check60MinTask,IsCheck60MinTask);
    }
    
    [RelayCommand]
    private async Task TestAction()
    {
        var selfRoomId = (await _biliService.GetRoomInfoAsync()).RoomId;
        
        var cookie = "CURRENT_QUALITY=0;b_lsid=3D9C828D_19966E7C879;theme-tip-show=SHOWED;home_feed_column=4;LIVE_BUVID=AUTO3917563974158518;buvid4=693757A4-F7AE-46F7-FD89-176D5864FF0465440-025080417-+Y68DEMsE7icBBnI4Ogsb8zMS4RLFD0sjE3sN1E7Y+p84Jp9uaGgje/3Dwd3EJRN;CURRENT_FNVAL=2000;buvid3=AAD37E52-3183-C9A7-8B21-A29BA301223E64690infoc;share_source_origin=COPY;sid=53fl154r;SESSDATA=921eeba8%2C1773900917%2C88c6b%2A91CjDngIQNz7HBaLpv599D40Uaey4Gl0GyJ09-mIbnwYoSaFDE_s3f1u5Es1naJ7zEBO4SVkZDdEFwdlJ2S1A2OHkzcEJoZUNPcm81cTlEem5WTWx6NTRLcm12QzJaRnpSRnlnR0VvWVJNRS15dWM5cjI4Q2Nnel9aUnNOdlhFWjFOVVBiZmlyZG13IIEC;bsource=share_source_copy_link;bmg_src_def_domain=i1.hdslb.com;theme-avatar-tip-show=SHOWED;b_nut=1754298164;_uuid=8F10F9DCF-3A93-1186-A31D-3E4D39CBACFB63157infoc;bili_jct=b262eb807ec125b7e6ac1c06c54aa26e;bili_ticket=eyJhbGciOiJIUzI1NiIsImtpZCI6InMwMyIsInR5cCI6IkpXVCJ9.eyJleHAiOjE3NTg2MTYzOTMsImlhdCI6MTc1ODM1NzEzMywicGx0IjotMX0.axlnDqIVwLHxwg9wx1YZxgZP4fLVTLMefLVAf2IKf9I;bili_ticket_expires=1758616333;bmg_af_switch=1;bp_t_offset_3493081477286220=1114632377969147904;browser_resolution=1385-744;buvid_fp=4973860c9239ec9f36c712d0d6a5b2e3;DedeUserID=3493081477286220;DedeUserID__ckMd5=5625cdd7363cee29;enable_web_push=DISABLE;hit-dyn-v2=1;PVID=1;rpdid=|(u|u~J~mu)~0J'u~l)JR)mku";

        // using var altService = new AltService(selfRoomId,cookie,"http://142.111.48.253:7030","pvcufsir","5ppw7212petm");
        
        // await giftService.SendDanmakuAsync("您好，这是一条测试弹幕");
        // await giftService.SendGiftAsync();
  
    }

    [RelayCommand]
    private async Task AddAltsAsync()
    {
        //弹出添加账号窗口
        using var altService = new AltService();
        var altVm = new AltsManagerViewModel(altService);
        await ShowWindowHelper.ShowWindowAsync(new AltsManager(){DataContext = altVm});
        
        if (altVm is { AllowDoneClose: true, CookieValue: {Length: > 0} cookie ,UserName: { Length: > 0 } userName})
        {
            
            // AltsList.Add(new Alt(cookie,userName,RemoveAlt));
           
            //保存账号到本地
            // await ConfigManager.AddAltAccountAsync(new AltAccount
            // {
            //     Cookie = altVm.CookieValue,
            //     UserName = userInfo.UserName,
            //     IsDanmakuSent = altVm.IsDanmakuSent,
            //     IsGiftSent = altVm.IsSendGift
            // });
        }
    }


    private void RemoveAlt(Alt alt)
    {
        if (AltsList.Contains(alt))
            AltsList.Remove(alt);
    }
}