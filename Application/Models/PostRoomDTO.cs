namespace Application.Models;

public class PostRoomDTO
{
    public required string RoomTypeID { get; set; }
    public required decimal PricePerNight { get; set; }
    public required int MaxOccupancy { get; set; }
    public required string RoomNumber { get; set; }
    public List<string> ImagesBase64 { get; set; } = new List<string>();
}
