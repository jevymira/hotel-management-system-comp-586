using Abstractions;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Reflection;

namespace LambdaASP.NETCore.Controllers;

[Authorize]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    AmazonDynamoDBClient _client;
    public RoomsController(IDBClientFactory<AmazonDynamoDBClient> factory)
    {
        _client = factory.GetClient();
    }

    [AllowAnonymous]
    [HttpGet("{id}")] // GET api/rooms/5XS34AD1LE
    public async Task<IActionResult> Get(string id)
    {
        var context = new DynamoDBContext(_client);
        Room room = await context.LoadAsync<Room>(id);
        return Ok(room);
    }

    [HttpGet] // GET api/rooms
    public async Task<IActionResult> Get()
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        return Ok(await context.ScanAsync<Room>(default).GetRemainingAsync());
    }

    /* sample body:
    {
        "roomTypeID": "Double",
        "roomNumber": "3",
        "pricePerNight": 150,
        "maxOccupancy": 2, // cannot be already in use
        "roomSize": "215 ft^2",
        "imageUrls": [ // sample, not final
            "https://h-images-group-4.s3.us-east-1.amazonaws.com/double-all.png",
            "https://h-images-group-4.s3.us-east-1.amazonaws.com/double-alt.png"
        ]
    }
    */
    [HttpPut] // PUT api/rooms
    public async Task<IActionResult> Put([FromBody] Room room)
    {
        // check if RoomNumber (separate from RoomID) is unique
        // (DynamoDB cannot enforce uniqueness on non-key attributes)
        var request = new QueryRequest
        {
            TableName = "Rooms",
            IndexName = "RoomNumber-index",
            KeyConditionExpression = "RoomNumber = :room_number",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":room_number", new AttributeValue(room.RoomNumber) }
            },
        };
        // query instead of get, because RoomNumber is a non-key attribute
        var data = await _client.QueryAsync(request);

        if (data.Count >= 1)
            return BadRequest("Room number already in use by another room.");
        else
        {
            Random random = new Random();
            var item = new Dictionary<string, AttributeValue>
            {
                {"RoomID", new AttributeValue(random.Next(100000000, 2147483647).ToString().PadLeft(10, '0')) },
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
    }

    /* sample body:
    {
        "roomTypeID": "Double",
        "pricePerNight": 150,
        "maxOccupancy": 2,
        "roomNumber": "3", // cannot be already in use by ANOTHER room
        "imageUrls": [
            "foo.bar",
            "bar.foo"
        ],
        "updatedBy": ""
    }
    */
    [HttpPatch("{id}")] // PATCH api/rooms/0123456789
    public async Task<IActionResult> Update(string id, [FromBody] Room room)
    {
        // check if RoomNumber (separate from RoomID) is unique
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
                { ":room_number", new AttributeValue(room.RoomNumber) },
                { ":room_id", new AttributeValue(id) }
            },
        };
        // query instead of GetItem because RoomNumber is a non-key attribute
        var data = await _client.QueryAsync(request);

        if (data.Count >= 1)
            return BadRequest("Room number already in use by another room.");
        else
        {
            var updateRequest = new UpdateItemRequest
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

            return Ok(await _client.UpdateItemAsync(updateRequest));
        }
    }
}