
using Amazon.DynamoDBv2;
using Application.Abstractions.Database;

namespace Application.DynamoDB;
public class DynamoDBClientFactory :
            IDBClientFactory<AmazonDynamoDBClient>
{
    private readonly IDBConnectionConfigFactory<AmazonDynamoDBConfig>
       _dataConnectionConfigFactory;

    public DynamoDBClientFactory(
       IDBConnectionConfigFactory<AmazonDynamoDBConfig>
          dataConnectionConfigFactory)
    {
        _dataConnectionConfigFactory = dataConnectionConfigFactory;
    }
    public AmazonDynamoDBClient GetClient()
    {
        AmazonDynamoDBConfig dynamoDBConfig =
            _dataConnectionConfigFactory.GetConfig();

        AmazonDynamoDBClient client = new AmazonDynamoDBClient(dynamoDBConfig);
        return client;
    }
}
