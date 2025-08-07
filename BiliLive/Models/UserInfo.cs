using Avalonia.Media.Imaging;

namespace BiliLive.Models;

public class UserInfo
{
    public Bitmap? UserFace { get; set; }
    public string? UserName { get; set; } = "Not login";
    public long UserId { get; set; } = 196431435;
}