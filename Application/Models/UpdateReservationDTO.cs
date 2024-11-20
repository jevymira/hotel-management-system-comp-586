namespace Application.Models;

public class UpdateReservationDTO
{
    public required string GuestFullName { get; set; }
    public required string GuestDateOfBirth {  get; set; }
    public required string GuestEmail {  get; set; }
    public string GuestPhoneNumber { get; set; } = string.Empty; // optional
    public required string ReservationStatus { get; set; }
    public List<string> RoomNumbers { get; set; } = new List<string>();
    public required string CheckInDate { get; set; }
    public required string CheckOutDate { get; set; }
    public required string UpdatedBy { get; set; }
}
