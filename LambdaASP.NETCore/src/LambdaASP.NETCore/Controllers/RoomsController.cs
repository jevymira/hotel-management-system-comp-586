using Abstractions;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LambdaASP.NETCore.Controllers;

[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    AmazonDynamoDBClient _client;
    public RoomsController(IDBClientFactory<AmazonDynamoDBClient> factory)
    {
        _client = factory.GetClient();
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        DynamoDBContext dbContext = new DynamoDBContext(_client);
        var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
        expressionAttributeValues.Add(":v_PK", "ROOM");

        var queryOperationConfig = new QueryOperationConfig
        {
            // IndexName = ,
            KeyExpression = new Expression
            {
                ExpressionStatement = "PK = :v_PK",
                ExpressionAttributeValues = expressionAttributeValues
            },
        };

        return Ok(await dbContext
                        .FromQueryAsync<Room>(queryOperationConfig)
                        .GetRemainingAsync());
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Room room)
    {
        string guid = $"ROOM#{System.Guid.NewGuid().ToString("D").ToUpper()}";
        var item = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue { S = "ROOM" },
            ["SK"] = new AttributeValue { S = guid },
            ["RoomTypeID"] = new AttributeValue { S = room.RoomTypeID },
            ["RoomNumber"] = new AttributeValue { S = room.RoomNumber },
            ["PricePerNight"] = new AttributeValue { N = room.PricePerNight.ToString() },
            ["MaxOccupancy"] = new AttributeValue { N = room.MaxOccupancy.ToString() },
            ["Status"] = new AttributeValue { S = room.Status },
            ["RoomSize"] = new AttributeValue { S = room.RoomSize },
            ["ImageUrls"] = new AttributeValue { SS = room.ImageUrls },
            ["UpdatedBy"] = new AttributeValue { S = String.Empty }
        };

        var putRequest = new PutItemRequest
        {
            TableName = "Hotel",
            Item = item
        };

        return Ok(await _client.PutItemAsync(putRequest));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] Room room)
    {
        var request = new UpdateItemRequest
        {
            TableName = "Hotel",
            Key = new Dictionary<string, AttributeValue>()
            {
                { "PK", new AttributeValue { S = "ROOM" } },
                { "SK", new AttributeValue { S = "ROOM#" + id } }
            },
            UpdateExpression = "SET RoomTypeID = :room_type_id, " +
                                   "PricePerNight = :price_per_night, " +
                                   "MaxOccupancy = :max_occupancy, " +
                                   "RoomNumber = :room_number, " +
                                   "ImageUrls = :image_urls, " +
                                   "UpdatedBy = :updated_by",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
            {
                { ":room_type_id", new AttributeValue(room.RoomTypeID) },
                { ":price_per_night", new AttributeValue(room.PricePerNight.ToString()) },
                { ":max_occupancy", new AttributeValue(room.MaxOccupancy.ToString()) },
                { ":room_number", new AttributeValue(room.RoomNumber) },
                { ":image_urls", new AttributeValue(room.ImageUrls) },
                { ":updated_by", new AttributeValue(room.UpdatedBy) }
            },
            // ReturnValues = ReturnValue.UPDATED_NEW
        };

        return Ok(await _client.UpdateItemAsync(request));
    }
}