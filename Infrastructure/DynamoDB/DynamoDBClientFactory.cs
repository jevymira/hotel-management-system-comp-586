using Infrastructure.Abstractions.Database;
using Amazon.DynamoDBv2;

namespace Infrastructure.DynamoDB;
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
