using Amazon.DynamoDBv2.DataModel;

namespace Domain.Entities;

/// <summary>
/// Hotel employee account for front-desk operations.
/// </summary>
[DynamoDBTable("AdminAccounts")]
public class AdminAccount
{
    [DynamoDBHashKey]
    public required string AdminID { get; set; }
    public required string FullName { get; set; }

    /// <summary>
    /// Employee email to be used at log in.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Employee password hash, to be verified against at log in.
    /// </summary>
    public required string PasswordHash { get; set; }
    public string AccountStatus { get; private set; }

    /// <summary>
    /// Set the account's AccountStatus to "Active."
    /// </summary>
    public void Activate()
    {
        AccountStatus = "Active";
    }

    /// <summary>
    /// Set the account's AccountStatus to "InActive."
    /// </summary>
    public void Deactivate()
    {
        AccountStatus = "InActive";
    }

    public bool IsActive()
    {
        return (AccountStatus.Equals("Active"));
    }
}
