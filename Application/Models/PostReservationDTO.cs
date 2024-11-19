namespace Application.Models;

public class PostReservationDTO
{
    public required string RoomType { get; set; }
    public required int OrderQuantity { get; set; }
    public required decimal TotalPrice { get; set; }
    public required string CheckInDate { get; set; }
    public required string CheckOutDate { get; set; }
    public required int NumberOfGuests { get; set; }
    public required string GuestFullName { get; set; }
    public required string GuestDateOfBirth { get; set; }
    public required string GuestEmail { get; set; }
    public string? GuestPhoneNumber { get; set; }
}
