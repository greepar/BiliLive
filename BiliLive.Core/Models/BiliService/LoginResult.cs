
namespace BiliLive.Core.Models.BiliService;

public class LoginResult { }

public class LoginSuccess : LoginResult
{
    public required string BiliCookie { get; init; }
    public required long UserId { get; init; }
    public required string UserName { get; init;}
    public required byte[] UserFaceBytes { get; init; }
}

public class LoginFailed : LoginResult
{
    public string? ErrorMsg { get; set;}
    public bool IsCanceled { get; set;}
    
}
