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
    public async Task<List<Room>> Get()
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        return await context.ScanAsync<Room>(default).GetRemainingAsync();
    }

    /* sample body:
    {
        "roomTypeID": "Double",
        "roomNumber": "3",
        "pricePerNight": 150,
        "maxOccupancy": 3,
        "roomSize": "215 ft^2",
        "imageUrls": [
            "foo.bar",
            "bar.foo"
        ]
    }
    */
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Room room)
    {
        Random random = new Random();
        var item = new Dictionary<string, AttributeValue>
        {
            { "RoomID", new AttributeValue(random.Next(100000000, 2147483647).ToString().PadLeft(10, '0')) },
            {"RoomTypeID", new AttributeValue(room.RoomTypeID) },
            {"RoomNumber", new AttributeValue(room.RoomNumber) },
            {"PricePerNight", new AttributeValue(room.PricePerNight.ToString()) },
            {"MaxOccupancy", new AttributeValue(room.MaxOccupancy.ToString()) },
            {"Status", new AttributeValue("Empty") },
            {"RoomSize", new AttributeValue(room.RoomSize) },
            {"ImageUrls", new AttributeValue(room.ImageUrls) },
            {"UpdatedBy", new AttributeValue(String.Empty) }
        };

        var putRequest = new PutItemRequest
        {
            TableName = "Rooms",
            Item = item
        };

        return Ok(await _client.PutItemAsync(putRequest));
    }

    /* sample body:
    {
        "roomTypeID": "Double",
        "pricePerNight": 150,
        "maxOccupancy": 3,
        "roomNumber": "1",
        "imageUrls": [
            "foo",
            "bar"
        ],
        "updatedBy": ""
    }
    */
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(string id, [FromBody] Room room)
    {
        var request = new UpdateItemRequest
        {
            TableName = "Rooms",
            Key = new Dictionary<string, AttributeValue>()
            {
                { "RoomID", new AttributeValue(id) }
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