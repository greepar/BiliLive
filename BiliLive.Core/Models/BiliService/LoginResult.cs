namespace BiliLive.Core.Models.BiliService;

public abstract class LoginResult { }

public class LoginSuccess : LoginResult
{
    public required long UserId { get; set; }
    public required string UserName { get; set;}
    public required string UserFaceUrl { get; set; }
}

public class LoginFailed : LoginResult
{
    public string? ErrorMsg { get; set;}
    public bool IsCanceled { get; set;}
    
}
