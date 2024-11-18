namespace Domain.Models;

public class CheckInOutDTO
{
    public required string ReservationStatus { get; set; }
    public List<string> RoomNumbers { get; set; } = new List<string>();
    public required string RoomStatus { get; set; }
}
