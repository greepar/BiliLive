namespace BiliLive.Core.Models.BiliService;

public class LiveRoomInfo
{
    public required byte[] RoomCover { get; set; }
    public required string Title { get; set; }
    public required long RoomId { get; set; }
}