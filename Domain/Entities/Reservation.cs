using Amazon.DynamoDBv2.DataModel;

namespace Domain.Entities;

[DynamoDBTable("Reservations")]
public class Reservation
{
    [DynamoDBHashKey]
    public required string ReservationID { get; set; }

    public required string RoomType { get; set; }

    public int OrderQuantity { get; set; }

    public List<string> RoomIDs { get; set; } = new List<string>(); // can be null

    public required string CheckInDate { get; set; }

    public required string CheckOutDate { get; set; }

    public int NumberOfGuests { get; set; }

    public decimal TotalPrice { get; set; }

    public required string BookingStatus { get; set; }

    public required string GuestFullName { get; set; }

    public required string GuestEmail { get; set; }

    public string? GuestPhoneNumber { get; set; }

    public required string GuestDateOfBirth { get; set; }

    public string? UpdatedBy { get; set; }

}
