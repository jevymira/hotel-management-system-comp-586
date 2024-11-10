using Amazon.DynamoDBv2.DataModel;

namespace Domain;

[DynamoDBTable("Rooms")]
public class Room
{
    [DynamoDBHashKey]
    public string? RoomID { get; set; }

    public string? RoomTypeID { get; set; }

    public string? RoomNumber { get; set; }

    public decimal? PricePerNight { get; set; }

    public int? MaxOccupancy { get; set; }

    public string? Status { get; set; }

    public string? RoomSize { get; set; }

    public List<string>? ImageUrls { get; set; }

    public string? UpdatedBy { get; set; }
}
