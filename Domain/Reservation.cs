using Amazon.DynamoDBv2.DataModel;

namespace Domain;

[DynamoDBTable("Hotel")]
public class Reservation
{
    [DynamoDBHashKey]
    public string? PK { get; set; }

    [DynamoDBRangeKey]
    public string? SK { get; set; }

    public string RoomID { get; set; }

    public string CheckInDate { get ; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey("PK-CheckOutDate-index")]
    public string CheckOutDate { get; set; }

    public int NumberOfGuests { get; set; }

    public decimal TotalPrice { get; set; }

    public string BookingStatus { get; set; }

    public string GuestFullName { get; set; }

    public string GuestEmail { get; set; }

    public string GuestPhoneNumber { get; set; }

    public string GuestDateOfBirth { get; set; }

    public string UpdatedBy { get; set; }

    /*public string? RoomType { get; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey("PK-StartDate-index")]
    public string? StartDate { get; set; }

    [DynamoDBGlobalSecondaryIndexRangeKey("PK-EndDate-index")]
    public string? EndDate { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public string? ConfirmationNumber { get; set; }

    public string? Status { get; set; }

    public string? RoomAssignment {  get; set; }*/
}
