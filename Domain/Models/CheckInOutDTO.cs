namespace Domain.Models;

public class CheckInOutDTO
{
    public string BookingStatus { get; set; }
    public List<string> RoomID { get; set; }
    public string RoomStatus { get; set; }
}
