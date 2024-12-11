using Amazon.DynamoDBv2.DataModel;

namespace Domain.Entities;

/// <summary>
/// Account Audit to review changes made to its corresponding account.
/// </summary>
[DynamoDBTable("AccountsStatus")]
public class AccountStatus
{
    [DynamoDBHashKey]
    public required string AuditID { get; set; }
    public required string AdminID { get; set; }
    public required string OldStatus { get; set; }
    public required string NewStatus { get; set; }
    public required string UpdatedBy { get; set; }
    public required string UpdateTime { get; set; }
}
