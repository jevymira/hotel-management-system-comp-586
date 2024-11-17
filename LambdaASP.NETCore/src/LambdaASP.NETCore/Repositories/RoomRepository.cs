using Abstractions;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using Domain.Abstractions.Repositories;
using Domain.Entities;
using Domain.Models;

namespace LambdaASP.NETCore.Repositories;

public class RoomRepository : IRoomRepository
{
    private readonly AmazonDynamoDBClient _client;

    public RoomRepository(IDBClientFactory<AmazonDynamoDBClient> factory)
    {
        _client = factory.GetClient();
    }

    public async Task<Room> SaveAsync(Room room)
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        await context.SaveAsync(room);
        return room;
    }

    public async Task<bool> RoomNumberExistsAsync(string id)
    {
        var request = new QueryRequest
        {
            TableName = "Rooms",
            IndexName = "RoomNumber-index",
            KeyConditionExpression = "RoomNumber = :room_number",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":room_number", new AttributeValue(id) }
            },
        };
        var data = await _client.QueryAsync(request);

        if (data.Count >= 1)
            return true;

        return false;
    }
}
