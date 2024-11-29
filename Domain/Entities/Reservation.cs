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

    public string CheckInDate { get; private set; }

    public string CheckOutDate { get; private set; }

    public int NumberOfGuests { get; set; }

    public decimal TotalPrice { get; set; }

    public string BookingStatus { get; private set; }

    public required string GuestFullName { get; set; }

    public required string GuestEmail { get; set; }

    public string GuestPhoneNumber { get; set; } = String.Empty; // can be null

    public required string GuestDateOfBirth { get; set; }

    public string UpdatedBy { get; set; } = String.Empty; // can be null

    // yyyy-MM-dd
    public void SetCheckInAndCheckOut(string checkInDate, string checkOutDate)
    {
        if (DateTime.Parse(checkInDate) > DateTime.Parse(checkOutDate))
            throw new ArgumentException("The check in date is after the check out date.");

        TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        DateTime now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, pacificZone);

        if (DateTime.Parse(checkInDate) < now || DateTime.Parse(checkOutDate) < now)
            throw new ArgumentException("A provided date has already passed.");

        if (DateTime.Parse(checkInDate) > now.AddYears(1) || DateTime.Parse(checkOutDate) > now.AddYears(1))
            throw new ArgumentException("Cannot book a year or more in advance.");

        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
    }

    public void SetDatesForExisting(string checkInDate, string checkOutDate)
    {
        if (DateTime.Parse(checkInDate) > DateTime.Parse(checkOutDate))
            throw new ArgumentException("The check in date is after the check out date.");

        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
    }

    public void CheckIn(List<Room> rooms) // may only be checked in if roomIDs are provided
    {
        if (rooms.Count == 0)
        {
            throw new ArgumentException("Either no rooms, the wrong room number(s), " +
                                        "or already occupied rooms were specified.");
        }

        List<string> ids = new List<string>();
        foreach (Room room in rooms)
        {
            ids.Add(room.RoomID);
        }
        RoomIDs = ids;

        BookingStatus = "Checked In";
    }

    public void CheckOut()
    {
        RoomIDs.Clear();
        BookingStatus = "Checked Out";
    }

    public void MarkDueIn()
    {
        RoomIDs.Clear();
        BookingStatus = "Due In";
    }

    public void MarkConfirmed()
    {
        RoomIDs.Clear();
        BookingStatus = "Confirmed";
    }

    public void MarkCancelled()
    {
        RoomIDs.Clear();
        BookingStatus = "Cancelled";
    }
}
