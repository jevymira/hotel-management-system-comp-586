using Infrastructure.Abstractions.Database;
using Amazon.DynamoDBv2;

namespace Infrastructure.DynamoDB;

public class DynamoDBConnectionConfigFactory :
             IDBConnectionConfigFactory<AmazonDynamoDBConfig>
{
    /// <summary>
    /// Produces a configuration for a DynamoDB client.
    /// </summary>
    /// <returns>AmazonDynamoDBConfig with pre-defined region.</returns>
    public AmazonDynamoDBConfig GetConfig()
    {
        AmazonDynamoDBConfig amazonDynamoDBConfig = new AmazonDynamoDBConfig();
        amazonDynamoDBConfig.ServiceURL = "https://dynamodb.us-east-1.amazonaws.com";
        return amazonDynamoDBConfig;
    }
}
