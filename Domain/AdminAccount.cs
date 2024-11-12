using Amazon.DynamoDBv2.DataModel;

namespace Domain;

[DynamoDBTable("AdminAccounts")]
public class AdminAccount
{
    [DynamoDBHashKey]
    public string? AdminID { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? AccountStatus { get; set; }

}
