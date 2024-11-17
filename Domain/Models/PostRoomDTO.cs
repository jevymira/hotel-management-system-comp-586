namespace Domain.Models;

public class PostRoomDTO
{
    public required string RoomTypeID { get; set; }
    public required decimal PricePerNight { get; set; }
    public required int MaxOccupancy { get; set; }
    public required string RoomNumber { get; set; }

    // new List<string>() returns 0, while null may not
    public required List<string> ImageUrls { get; set; } = new List<string>();
}
