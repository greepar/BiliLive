namespace BiliLive.Core.Models.BiliService;

public class LoginResult
{
    public bool? IsSuccess { get; set; }
    public long? UserId { get; set; } 
    public string? UserName { get; set; }
    public string? UserFaceUrl { get; set; }
    public string? ErrorMsg { get; set; }
}