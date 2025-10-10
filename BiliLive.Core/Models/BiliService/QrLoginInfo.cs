namespace BiliLive.Core.Models.BiliService;

public class QrLoginInfo
{
    public required string QrCodeUrl { get; init; }
    public required string QrCodeKey { get; init; }
}