namespace BiliLive.Core.Models.BiliService;

public class LiveRoomInfo
{
    public required byte[] RoomCover { get; init; }
    public required string Title { get; init; }
    public required long RoomId { get; init; }
}