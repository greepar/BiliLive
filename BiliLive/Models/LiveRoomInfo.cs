using Avalonia.Media.Imaging;

namespace BiliLive.Models;

public class LiveRoomInfo
{
    public Bitmap? RoomCover { get; set; }
    public string? RoomTitle { get; set; }
    public int RoomAreaId { get; set; }
    public string? ApiKey { get; set; }
}