namespace Application.Models;

public class ReservationDTO
{
    public required string ReservationID { get; set; }

    public required string RoomType { get; set; }

    public int OrderQuantity { get; set; }

    public List<string> RoomNumbers { get; set; } = new List<string>(); // can be null

    public string CheckInDate { get; set; }

    public string CheckOutDate { get; set; }

    public int NumberOfGuests { get; set; }

    public decimal TotalPrice { get; set; }

    public string BookingStatus { get; set; }

    public required string GuestFullName { get; set; }

    public required string GuestEmail { get; set; }

    public string GuestPhoneNumber { get; set; } = String.Empty; // can be null

    public required string GuestDateOfBirth { get; set; }

    public string UpdatedBy { get; set; } = String.Empty; // can be null
}
