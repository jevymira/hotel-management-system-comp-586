using Abstractions;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Domain.Abstractions.Services;
using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LambdaASP.NETCore.Controllers;

[Authorize]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly AmazonDynamoDBClient _client;
    private readonly IRoomService _roomService;
    private readonly IImageService _imageService;
    public RoomsController(IDBClientFactory<AmazonDynamoDBClient> factory, 
        IRoomService roomService, IImageService imageService)
    {
        _client = factory.GetClient();
        _roomService = roomService;
        _imageService = imageService;
    }

    [AllowAnonymous]
    [HttpGet("{id}")] // GET api/rooms/0123456789
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

    // request Header: ( Key: Content-Type, Value: multipart/form-data; boundary=<parameter> )
    // request Body:
    //   form-data for content-type: application/json
    //     RoomDTO[roomTypeID], RoomDTO[maxOccupancy], RoomDTO[pricePerNight], RoomDTO[roomNumber]
    //   form-data for content-type: multipart/form-data
    //     images
    [HttpPost] // POST api/rooms
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Put([FromForm] PostRoomDTO roomDTO,
        [FromForm(Name = "images")] List<IFormFile> images)
    {
        Room room;

        try
        {
            room = await _roomService.CreateAsync(roomDTO, images);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }

        return CreatedAtAction(nameof(Get), new { id = room.RoomID }, value: room);
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