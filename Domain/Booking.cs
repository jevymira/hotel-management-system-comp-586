using Amazon.DynamoDBv2.DataModel;

namespace Domain;

[DynamoDBTable("Hotel")]
public class Booking
{
    [DynamoDBHashKey]
    public string? PK { get; set; }

    [DynamoDBRangeKey]
    public string? SK { get; set; }

    public string? RoomType { get; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey("PK-StartDate-index")]
    public string? StartDate { get; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey("PK-EndDate-index")]
    public string? EndDate { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? ConfirmationNumber { get; set; }

    public string? Status { get; set; }

    public string? RoomAssignment {  get; set; }
}
