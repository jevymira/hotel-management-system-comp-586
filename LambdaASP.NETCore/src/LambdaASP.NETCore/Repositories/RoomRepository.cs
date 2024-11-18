using Abstractions;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Domain.Abstractions.Repositories;
using Domain.Entities;
using Domain.Models;
using System.Reflection;

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
        var context = new DynamoDBContext(_client);
        await context.SaveAsync(room);
        return room;
    }
    public async Task<Room> LoadRoomAsync(string id)
    {
        var context = new DynamoDBContext(_client);
        return await context.LoadAsync<Room>(id);
    }

    public async Task<List<Room>> ScanAsync()
    {
        var context = new DynamoDBContext(_client);
        return await context.ScanAsync<Room>(default).GetRemainingAsync();
    }

    public async Task UpdateAsync(string id, UpdateRoomDTO roomDTO, List<string> urls)
    {
        var request = new UpdateItemRequest
        {
            TableName = "Rooms",
            Key = new Dictionary<string, AttributeValue>()
            {
                { "RoomID", new AttributeValue(id) }
            },
            UpdateExpression =
            (
                "SET RoomTypeID = :room_type_id, " +
                    "PricePerNight = :price_per_night, " +
                    "MaxOccupancy = :max_occupancy, " +
                    "RoomNumber = :room_number, " +
                    "ImageUrls = :image_urls, " +
                    "UpdatedBy = :updated_by"
            ),
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
            {
                { ":room_type_id", new AttributeValue(roomDTO.RoomTypeID) },
                { ":price_per_night", new AttributeValue(roomDTO.PricePerNight.ToString()) },
                { ":max_occupancy", new AttributeValue(roomDTO.MaxOccupancy.ToString()) },
                { ":room_number", new AttributeValue(roomDTO.RoomNumber) },
                { ":image_urls", new AttributeValue(urls) },
                { ":updated_by", new AttributeValue(roomDTO.UpdatedBy) }
            },
        };

        await _client.UpdateItemAsync(request);

    }

    public async Task<bool> RoomIdExistsAsync(string id)
    {
        var request = new QueryRequest
        {
            TableName = "Rooms",
            KeyConditionExpression = "RoomID = :room_id",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":room_id", new AttributeValue(id) }
            },
        };
        var response = await _client.QueryAsync(request);

        return (response.Count > 0);
    }

    public async Task<bool> RoomNumberExistsAsync(string num)
    {
        // (DynamoDB cannot enforce uniqueness on non-key attributes)
        var request = new QueryRequest
        {
            TableName = "Rooms",
            IndexName = "RoomNumber-index",
            KeyConditionExpression = "RoomNumber = :room_number",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":room_number", new AttributeValue(num) }
            },
        };
        // query because GetItem cannot be performed on a non-key attribute
        var response = await _client.QueryAsync(request);

        return (response.Count > 0);
    }

    public async Task<bool> RoomNumberExistsElsewhereAsync(string num, string id)
    {
        // (DynamoDB cannot enforce uniqueness on non-key attributes)
        var request = new QueryRequest
        {
            TableName = "Rooms",
            IndexName = "RoomNumber-index",
            KeyConditionExpression = "RoomNumber = :room_number",
            FilterExpression = "RoomID <> :room_id",
            // disqualifies the Item to-be-updated for the count
            // (not equals operator not available for KeyConditionExpression)
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":room_number", new AttributeValue(num) },
                { ":room_id", new AttributeValue(id) }
            },
        };
        // query because GetItem cannot be performed on a non-key attribute
        var response = await _client.QueryAsync(request);

        return (response.Count > 0);
    }

    public async Task<Room> QueryByRoomNumberAsync(string num)
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        var cfg = new DynamoDBOperationConfig { IndexName = "RoomNumber-index" };
        var room = await context.QueryAsync<Room>(num, cfg).GetRemainingAsync();
        return room.Single();
    }
}
