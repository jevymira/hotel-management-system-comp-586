using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain.Abstractions.Repositories;
using Domain.Entities;
using Domain.Models;
using Infrastructure.Abstractions.Database;

namespace Infrastructure.Repositories;

/// <summary>
/// Encapsulates the logic for the retrieval/persistence of admin accounts.
/// </summary>
public class AdminAccountRepository : IAdminAccountRepository
{
    AmazonDynamoDBClient _client;

    public AdminAccountRepository(IDBClientFactory<AmazonDynamoDBClient> factory)
    {
        _client = factory.GetClient();
    }

    public async Task SaveAsync(AdminAccount account)
    {
        var context = new DynamoDBContext(_client);
        await context.SaveAsync(account);
    }

    public async Task<AdminAccount> LoadAsync(string id)
    {
        var context = new DynamoDBContext(_client);
        return await context.LoadAsync<AdminAccount>(id);
    }

    /// <summary>
    /// Retrieve the account, specified by its ID, but with its password hash omitted.
    /// </summary>
    /// <param name="id">ID of account to retrieve.</param>
    public async Task<GetAdminAccountDTO> LoadForClientAsync(string id)
    {
        var context = new DynamoDBContext(_client);
        return await context.LoadAsync<GetAdminAccountDTO>(id);
    }
    public async Task<List<GetAdminAccountDTO>> LoadAllForClientAsync()
    {
        var context = new DynamoDBContext(_client);
        return await context.ScanAsync<GetAdminAccountDTO>(default).GetRemainingAsync();
    }

    public async Task<bool> QueryIfEmailExists(string email)
    {
        // (DynamoDB cannot enforce uniqueness on non-key attributes)
        var request = new QueryRequest
        {
            TableName = "AdminAccounts",
            IndexName = "Email-PasswordHash-index",
            KeyConditionExpression = "Email = :email",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":email", new AttributeValue(email) }
            },
        };
        // query because GetItem cannot be performed on a non-key attribute
        var result = await _client.QueryAsync(request);
        return (result.Count > 0);
    }

    /// <summary>
    /// Query whether an email exists, excluding the specified account (by ID).
    /// </summary>
    /// <param name="email">Email to query on.</param>
    /// <param name="id">ID of account to ignore.</param>
    /// <returns>True if email exists outside of ignored account.</returns>
    public async Task<bool> QueryIfEmailExists(string email, string id)
    {
        // (DynamoDB cannot enforce uniqueness on non-key attributes)
        var request = new QueryRequest
        {
            TableName = "AdminAccounts",
            IndexName = "Email-PasswordHash-index",
            KeyConditionExpression = "Email = :email",
            FilterExpression = "AdminID <> :admin_id",
            // disqualifies the Item to-be-updated for the count
            // (not equals operator not available for KeyConditionExpression)
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":email", new AttributeValue(email) },
                { ":admin_id", new AttributeValue(id) }
            },
        };
        // query because GetItem cannot be performed on a non-key attribute
        var response = await _client.QueryAsync(request);

        return response.Count > 0;
    }

    /// <summary>
    /// Query account with email/password combination and which additionaly is Active.
    /// </summary>
    /// <param name="email">Admin account email.</param>
    /// <param name="passwordHash">Admin account password hash.</param>
    public async Task<AdminAccount?> QueryAccountByCredentialsIfActive(string email, string passwordHash)
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        var cfg = new DynamoDBOperationConfig
        {
            IndexName = "Email-PasswordHash-index",
            QueryFilter = new List<ScanCondition>()
            {
                new ScanCondition("PasswordHash", ScanOperator.Equal, passwordHash),
                new ScanCondition("AccountStatus", ScanOperator.Equal, "Active")
            }
        };
        var result = await context.QueryAsync<AdminAccount>(email, cfg).GetRemainingAsync();
        // uniqueness enforced by business logic, cannot in DynamoDB
        if (result.Count != 0)
            return result.Single(); // InvalidOperationException if null
        return null;
    }

    public async Task<AdminAccount?> QueryAccountByCredentials(string email, string passwordHash)
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        var cfg = new DynamoDBOperationConfig
        {
            IndexName = "Email-PasswordHash-index",
            QueryFilter = new List<ScanCondition>() 
            {
                new ScanCondition("PasswordHash", ScanOperator.Equal, passwordHash)
            }
        };
        var result = await context.QueryAsync<AdminAccount>(email, cfg).GetRemainingAsync();
        // uniqueness enforced by business logic, cannot in DynamoDB
        if (result.Count != 0)
            return result.Single(); // InvalidOperationException if null
        return null;
    }

    public async Task<string?> QueryAuditIDByAccountID(string id)
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        var cfg = new DynamoDBOperationConfig
        {
            IndexName = "AdminID-index",
            QueryFilter = new List<ScanCondition>()
            {
                new ScanCondition("AdminID", ScanOperator.Equal, id)
            }
        };
        var result = await context.QueryAsync<AccountStatus>(id, cfg).GetRemainingAsync();
        if (result.Count != 0)
            return result.Single().AuditID; // InvalidOperationException if null
        return null;
    }

    public async Task UpdateDetailsAsync(AdminAccount account, string auditID, string updatedBy)
    {
        TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        DateTime now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, pacificZone);

        var request = new TransactWriteItemsRequest()
        {
            TransactItems = new List<TransactWriteItem>()
            {
                new TransactWriteItem()
                {
                    Update = new Update()
                    {
                        TableName = "AdminAccounts",
                        Key = new Dictionary<string, AttributeValue>
                        {
                            { "AdminID", new AttributeValue(account.AdminID) },
                        },
                        UpdateExpression =
                        (
                            "SET FullName = :full_name, " +
                                "Email = :email, " +
                                "AccountStatus = :account_status"
                        ),
                        ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                        {
                            { ":full_name", new AttributeValue(account.FullName) },
                            { ":email", new AttributeValue(account.Email) },
                            { ":account_status", new AttributeValue(account.AccountStatus) }
                        }
                    }
                },
                new TransactWriteItem()
                {
                    Put = new Put()
                    {
                        TableName = "AccountsStatus",
                        Item = new Dictionary<string, AttributeValue>
                        {
                            {"AuditID", new AttributeValue(auditID)},
                            {"AdminID", new AttributeValue(account.AdminID)},
                            {"OldStatus",new AttributeValue(account.IsActive() ? "InActive" : "Active")},
                            {"NewStatus",new AttributeValue(account.AccountStatus)},
                            {"UpdatedBy",new AttributeValue(updatedBy)},
                            {"UpdatedTime",new AttributeValue(now.ToString())}
                        }
                    }
                }
            },
        };

        await _client.TransactWriteItemsAsync(request);

        //var req = new UpdateItemRequest
        //{
        //    TableName = "AdminAccounts",
        //    Key = new Dictionary<string, AttributeValue>()
        //    {
        //        { "AdminID", new AttributeValue(account.AdminID) }
        //    },
        //    UpdateExpression =
        //    (
        //        "SET FullName = :full_name, " +
        //            "Email = :email, " +
        //            "AccountStatus = :account_status"
        //    ),
        //    ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
        //    {
        //        { ":full_name", new AttributeValue(account.FullName) },
        //        { ":email", new AttributeValue(account.Email) },
        //        { ":account_status", new AttributeValue(account.AccountStatus) }
        //    }
        //};
        //await _client.UpdateItemAsync(req);
    }

    public async Task UpdatePasswordAsync(string id, string newPasswordHash)
    {
        var request = new UpdateItemRequest
        {
            TableName = "AdminAccounts",
            Key = new Dictionary<string, AttributeValue>()
            {
                { "AdminID", new AttributeValue(id) }
            },
            UpdateExpression =
            (
                "SET PasswordHash = :password_hash"
            ),
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
            {
                { ":password_hash", new AttributeValue(newPasswordHash) },
            }
        };
        await _client.UpdateItemAsync(request);
    }
}
