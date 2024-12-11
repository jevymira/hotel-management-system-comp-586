using Amazon.DynamoDBv2.DataModel;

namespace Domain.Entities;

/// <summary>
/// Reservation spanning two dates for the specified quantity of a type of room.
/// </summary>
[DynamoDBTable("Reservations")]
public class Reservation
{
    [DynamoDBHashKey]
    public required string ReservationID { get; set; }

    /// <summary>
    /// RoomType, from the set: Single, Double, Triple, Quad
    /// </summary>
    public required string RoomType { get; set; }

    /// <summary>
    /// Quantity of rooms, of the type defined by RoomType
    /// </summary>
    public int OrderQuantity { get; set; }

    public List<string> RoomIDs { get; set; } = new List<string>(); // can be null

    public string CheckInDate { get; private set; }

    public string CheckOutDate { get; private set; }

    public int NumberOfGuests { get; set; }

    public decimal TotalPrice { get; set; }

    /// <summary>
    /// The booking status to be changed once due in, at check in, at check out, etc.
    /// </summary>
    public string BookingStatus { get; private set; }

    public required string GuestFullName { get; set; }

    public required string GuestEmail { get; set; }

    public string GuestPhoneNumber { get; set; } = String.Empty; // can be null

    public required string GuestDateOfBirth { get; set; }

    public string UpdatedBy { get; set; } = String.Empty; // can be null

    /// <summary>
    /// Sets check in and check out simultaneously for the reservation to ensure
    /// consistency and adherence to business logic.
    /// </summary>
    /// <param name="checkInDate">Date on which reservation is to be checked in.</param>
    /// <param name="checkOutDate">Date on which reservation is to be checked out.</param>
    public void SetCheckInAndCheckOut(string checkInDate, string checkOutDate)
    {
        if (DateTime.Parse(checkInDate) > DateTime.Parse(checkOutDate))
            throw new ArgumentException("The check in date is after the check out date.");

        TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        DateTime now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, pacificZone);

        if (DateTime.Parse(checkInDate).Date < now.Date || DateTime.Parse(checkOutDate).Date < now.Date)
            throw new ArgumentException("A provided date has already passed.");

        if (DateTime.Parse(checkInDate) > now.AddYears(1) || DateTime.Parse(checkOutDate) > now.AddYears(1))
            throw new ArgumentException("Cannot book a year or more in advance.");

        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
    }

    /// <summary>
    /// Update (or retain) check in and check out dates for an existing reservation,
    /// with relaxed checks compared to new reservations.
    /// </summary>
    /// <param name="checkInDate"></param>
    /// <param name="checkOutDate"></param>
    /// <exception cref="ArgumentException"></exception>
    public void SetDatesForExisting(string checkInDate, string checkOutDate)
    {
        if (DateTime.Parse(checkInDate) > DateTime.Parse(checkOutDate))
            throw new ArgumentException("The check in date is after the check out date.");

        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
    }

    /// <summary>
    /// Check in the reservation, providing it room assignments (which, 
    /// if absent, constitutes a violation of business logic).
    /// </summary>
    /// <param name="rooms">Rooms to be assigned to the reservation.</param>
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

    /// <summary>
    /// Check out the reservation, 
    /// clearing it of its room assignments in the process.
    /// </summary>
    public void CheckOut()
    {
        RoomIDs.Clear();
        BookingStatus = "Checked Out";
    }

    /// <summary>
    /// Mark a reservation Due in, clearing it of its room assignments
    /// (if any) in the process.
    /// </summary>
    public void MarkDueIn()
    {
        RoomIDs.Clear();
        BookingStatus = "Due In";
    }

    /// <summary>
    /// Mark a reservation Confirmed, clearing it of its room assignments 
    /// (if any) in the process.
    /// </summary>
    public void MarkConfirmed()
    {
        RoomIDs.Clear();
        BookingStatus = "Confirmed";
    }

    /// <summary>
    /// Cancel a reservation, clearing it of its room assignments 
    /// (if any) in the process.
    /// </summary>
    public void MarkCancelled()
    {
        RoomIDs.Clear();
        BookingStatus = "Cancelled";
    }
}
