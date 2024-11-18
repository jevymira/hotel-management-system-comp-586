using Abstractions;
using Amazon.DynamoDBv2.DataModel;

namespace Domain.Entities;

[DynamoDBTable("Reservations")]
public class Reservation
{
    [DynamoDBHashKey("ReservationID")]
    // [DynamoDBGlobalSecondaryIndexRangeKey("GuestFullName-ReservationID-index")]
    public string? ReservationID { get; set; }

    public string? RoomType { get; set; }

    public int? OrderQuantity { get; set; }

    public List<string> RoomIDs { get; set; } = new List<string>();

    // [DynamoDBGlobalSecondaryIndexRangeKey("BookingStatus-CheckInDate-index")]
    public string? CheckInDate { get; set; }

    public string? CheckOutDate { get; set; }

    public int? NumberOfGuests { get; set; }

    public decimal? TotalPrice { get; set; }

    // [DynamoDBGlobalSecondaryIndexHashKey("BookingStatus-CheckInDate-index")]
    public string? BookingStatus { get; set; }

    // [DynamoDBGlobalSecondaryIndexHashKey("GuestFullName-ReservationID-index")]
    public string? GuestFullName { get; set; }

    public string? GuestEmail { get; set; }

    public string? GuestPhoneNumber { get; set; }

    public string? GuestDateOfBirth { get; set; }

    public string? UpdatedBy { get; set; }

}
