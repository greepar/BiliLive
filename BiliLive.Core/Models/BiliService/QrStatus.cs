namespace BiliLive.Core.Models.BiliService;

public abstract class QrStatus
{
    public enum StatusList
    {
        Expired,Scanned,Confirmed,Error
    }
    
    public required StatusList Status { get; set; }
    
    public string? FaceUrl { get; set; }
    public string? Username { get; set; }
    public string? UserId { get; set; }
    
}