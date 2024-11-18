using Amazon.DynamoDBv2.DataModel;

namespace Domain.Models;

[DynamoDBTable("AdminAccounts")]
public class GetAdminAccountDTO
{
    [DynamoDBHashKey]
    public required string AdminID { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string AccountStatus { get; set; }
}
