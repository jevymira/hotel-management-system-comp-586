using Amazon.DynamoDBv2.DataModel;
using Domain.Services;
using System.Globalization;

namespace Domain.Entities;

[DynamoDBTable("Reservations")]
public class Reservation
{
    [DynamoDBHashKey]
    public required string ReservationID { get; set; }

    public required string RoomType { get; set; }

    public int OrderQuantity { get; set; }

    public List<string> RoomIDs { get; set; } = new List<string>(); // can be null

    public string CheckInDate { get; private set; }

    public string CheckOutDate { get; private set; }

    public int NumberOfGuests { get; set; }

    public decimal TotalPrice { get; set; }

    public required string BookingStatus { get; set; }

    public required string GuestFullName { get; set; }

    public required string GuestEmail { get; set; }

    public string? GuestPhoneNumber { get; set; }

    public required string GuestDateOfBirth { get; set; }

    public string? UpdatedBy { get; set; }

    // yyyy-MM-dd
    public void SetCheckInAndCheckOutDate(string checkInDate, string checkOutDate)
    {
        if (DateTime.Parse(checkInDate) > DateTime.Parse(checkOutDate))
            throw new ArgumentException("The check in date is after the check out date.");

        TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        DateTime now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, pacificZone);

        if (DateTime.Parse(checkInDate) < now || DateTime.Parse(checkOutDate) < now)
            throw new ArgumentException("A provided date has already passed.");

        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
    }

    public void CheckIn(List<string> roomIDs) // may only be checked in if roomIDs are provided
    {
        if (roomIDs.Count == 0)
        {
            throw new ArgumentException("Either no rooms, the wrong room number(s), " +
                                        "or already occupied rooms were specified.");
        }
        RoomIDs = roomIDs;
        BookingStatus = "Checked In";
    }

    public void CheckOut()
    {
        RoomIDs.Clear();
        BookingStatus = "Checked Out";
    }

}
