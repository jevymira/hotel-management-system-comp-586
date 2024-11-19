using Amazon.DynamoDBv2.DataModel;

namespace Domain.Entities;

[DynamoDBTable("Rooms")]
public class Room
{
    [DynamoDBHashKey]
    public required string RoomID { get; set; }

    public required string RoomTypeID { get; set; }

    public required string RoomNumber { get; set; }

    public required decimal PricePerNight { get; set; }

    public required int MaxOccupancy { get; set; }

    public required string Status { get; set; }

    public required string RoomSize { get; set; }

    public required List<string> ImageUrls { get; set; }

    public string? UpdatedBy { get; set; } // nullable
}
