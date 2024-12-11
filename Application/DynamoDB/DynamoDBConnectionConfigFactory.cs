using Amazon.DynamoDBv2;
using Application.Abstractions.Database;

namespace Application.DynamoDB;

public class DynamoDBConnectionConfigFactory :
             IDBConnectionConfigFactory<AmazonDynamoDBConfig>
{
    public AmazonDynamoDBConfig GetConfig()
    {
        AmazonDynamoDBConfig amazonDynamoDBConfig = new AmazonDynamoDBConfig();
        amazonDynamoDBConfig.ServiceURL = "https://dynamodb.us-east-1.amazonaws.com";
        return amazonDynamoDBConfig;
    }
}
