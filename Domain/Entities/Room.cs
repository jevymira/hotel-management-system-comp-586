using Amazon.DynamoDBv2.DataModel;

namespace Domain.Entities;

/// <summary>
/// Hotel room, defining attributes for max occupancy, price, and
/// publicly-accessible image urls.
/// </summary>
[DynamoDBTable("Rooms")]
public class Room
{
    [DynamoDBHashKey]
    public required string RoomID { get; set; }

    public required string RoomTypeID { get; set; }

    /// <summary>
    /// Unique Room Number, separate from RoomID, that is 
    /// provided to guests and used by desk staff.
    /// </summary>
    public required string RoomNumber { get; set; }

    public required decimal PricePerNight { get; set; }

    public required int MaxOccupancy { get; set; }
    
    /// <summary>
    /// Room status, to be updated when occupied/vacated.
    /// </summary>
    public string Status { get; private set; }

    public required string RoomSize { get; set; }

    /// <summary>
    /// URLs for hosted room images.
    /// </summary>
    public required List<string> ImageUrls { get; set; }

    public string UpdatedBy { get; set; } = String.Empty; // can be null

    /// <summary>
    /// Assign the Status attribute "Empty."
    /// </summary>
    public void MarkEmpty()
    {
        Status = "Empty";
    }

    /// <summary>
    /// Assign the Status attribute "Occupied."
    /// </summary>
    public void MarkOccupied()
    {
        Status = "Occupied";
    }

    public bool IsOccupied()
    {
        return Status.Equals("Occupied");
    }
}
