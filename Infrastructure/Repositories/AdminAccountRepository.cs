using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain;
using Domain.Abstractions.Repositories;
using Domain.Entities;
using Domain.Models;
using Infrastructure.Abstractions.Database;

namespace Infrastructure.Repositories;

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

    public async Task<GetAdminAccountDTO> LoadAsync(string id)
    {
        var context = new DynamoDBContext(_client);
        return await context.LoadAsync<GetAdminAccountDTO>(id);
    }
    public async Task<List<GetAdminAccountDTO>> LoadAllAsync()
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

    public async Task UpdateDetailsAsync(string id, UpdateAdminAccountDTO dto)
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
                "SET FullName = :full_name, " +
                    "Email = :email, " +
                    "AccountStatus = :account_status"
            ),
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
            {
                { ":full_name", new AttributeValue(dto.FullName) },
                { ":email", new AttributeValue(dto.Email) },
                { ":account_status", new AttributeValue(dto.AccountStatus) }
            }
        };
        await _client.UpdateItemAsync(request);
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
