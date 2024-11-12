using Abstractions;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Globalization;
using LambdaASP.NETCore.Models;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;

namespace LambdaASP.NETCore.Controllers;

[Authorize]
[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    AmazonDynamoDBClient _client;

    public ReservationsController(IDBClientFactory<AmazonDynamoDBClient> factory)
    {
        _client = factory.GetClient();
    }

    [AllowAnonymous]
    [HttpGet("by-id/{id}")] // GET api/reservations/by-id/0123456789
    public async Task<IActionResult> Get(string id)
    {
        var context = new DynamoDBContext(_client);
        Reservation res = await context.LoadAsync<Reservation>(id);
        return Ok(res);
    }

    [HttpGet("by-name/{name}")] // GET api/reservations/by-name/John%20Doe
    public async Task<IActionResult> GetByName(string name)
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
        expressionAttributeValues.Add(":v_name", name);
        var queryOperationConfig = new QueryOperationConfig
        {
            IndexName = "GuestFullName-ReservationID-index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "GuestFullName = :v_name",
                ExpressionAttributeValues = expressionAttributeValues
            }
        };

        return Ok(await context.FromQueryAsync<Reservation>(queryOperationConfig).GetRemainingAsync());
    }

    // TODO: refactor
    // returns in order of:
    // all due in reservations
    // all checked in reservations
    // those checked out reservations of the current date
    // those confirmed reservations with a check in date from the current date onward
    [HttpGet]
    public async Task<List<Reservation>> QueryByDate()
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        string date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, pacificZone).ToString("yyyy-MM-dd");

        var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
        expressionAttributeValues.Add(":v_status", "Due In");

        var query = new QueryOperationConfig
        {
            IndexName = "BookingStatus-CheckInDate-index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "BookingStatus = :v_status", // AND CheckInDate >= :v_date",
                ExpressionAttributeValues = expressionAttributeValues
            }
        };

        var data = await context.FromQueryAsync<Reservation>(query).GetRemainingAsync();


        expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
        expressionAttributeValues.Add(":v_status", "Checked In");

        query = new QueryOperationConfig
        {
            IndexName = "BookingStatus-CheckOutDate-index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "BookingStatus = :v_status",
                ExpressionAttributeValues = expressionAttributeValues
            }
        };

        data.AddRange(await context.FromQueryAsync<Reservation>(query).GetRemainingAsync());


        expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
        expressionAttributeValues.Add(":v_status", "Checked Out");
        expressionAttributeValues.Add(":v_date", date);

        query = new QueryOperationConfig
        {
            IndexName = "BookingStatus-CheckOutDate-index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "BookingStatus = :v_status AND CheckOutDate = :v_date",
                ExpressionAttributeValues = expressionAttributeValues
            }
        };

        data.AddRange(await context.FromQueryAsync<Reservation>(query).GetRemainingAsync());


        expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
        expressionAttributeValues.Add(":v_status", "Confirmed");
        expressionAttributeValues.Add(":v_date", date);

        query = new QueryOperationConfig
        {
            IndexName = "BookingStatus-CheckInDate-index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "BookingStatus = :v_status AND CheckInDate >= :v_date",
                ExpressionAttributeValues = expressionAttributeValues
            }
        };

        data.AddRange(await context.FromQueryAsync<Reservation>(query).GetRemainingAsync());

        return data;
    }

    /* sample request body
    {
        "CheckInDate": "2024-12-01",
        "CheckOutDate": "2024-12-03",
        "NumberOfGuests": 2,
        "TotalPrice" : 150.00,
        "GuestFullName": "John Doe",
        "GuestEmail": "jdoe@email.com",
        "GuestPhoneNumber": "(555) 555-5555",
        "GuestDateOfBirth": "1980-01-01"
    } 
    */
    [HttpPut] // PUT api/bookings
    public async Task<string> CreateReservation([FromBody] Reservation reservation)
    {
        Random random = new Random();
        string id = random.Next(100000000, 2147483647).ToString().PadLeft(10, '0');

        var item = new Dictionary<string, AttributeValue>
        {
            { "ReservationID", new AttributeValue(id) },
            { "RoomID", new AttributeValue(String.Empty) },
            { "CheckInDate", new AttributeValue(reservation.CheckInDate) },
            { "CheckOutDate", new AttributeValue(reservation.CheckOutDate) },
            { "NumberOfGuests", new AttributeValue(reservation.NumberOfGuests.ToString()) },
            { "TotalPrice", new AttributeValue(reservation.TotalPrice.ToString()) },
            { "BookingStatus", new AttributeValue("Confirmed") },
            { "GuestFullName", new AttributeValue(reservation.GuestFullName) },
            { "GuestEmail", new AttributeValue(reservation.GuestEmail) },
            { "GuestPhoneNumber", new AttributeValue(reservation.GuestPhoneNumber) },
            { "GuestDateOfBirth", new AttributeValue(reservation.GuestDateOfBirth) },
            { "UpdatedBy", new AttributeValue(String.Empty) }
        };

        var putRequest = new PutItemRequest
        {
            TableName = "Reservations",
            Item = item
        };

        await _client.PutItemAsync(putRequest);

        return id;
    }

    /* sample request body
    {
        "bookingStatus": "Checked In",
        "RoomID": [
            "MOCK1",
            "MOCK2"
        ],
        "RoomStatus": "Occupied"
    }
    */
    [HttpPatch("{id}")]
    public async Task<IActionResult> CheckReservation(
        string id, [FromBody] CheckInOutDTO model)
    {
        List<TransactWriteItem> list = new List<TransactWriteItem>()
        {
            new TransactWriteItem()
            {
                Update = new Update()
                {
                    TableName = "Reservations",
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "ReservationID", new AttributeValue(id) }
                    },
                    UpdateExpression = "SET BookingStatus = :status, " +
                                            "RoomID = :rooms",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":status", new AttributeValue(model.BookingStatus) },
                        { ":rooms", new AttributeValue(model.RoomID) }
                    },
                }
            },
        };

        for (int i = 0; i < model.RoomID.Count(); i++)
        {
            list.Add(new TransactWriteItem()
            {
                Update = new Update()
                {
                    TableName = "Rooms",
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "RoomID", new AttributeValue(model.RoomID[i]) }
                    },
                    UpdateExpression = "SET #s = :status",
                    ExpressionAttributeNames = new Dictionary<string, string>
                    {
                        { "#s", "Status" }, // alias, status is a reserved word
                    },
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":status", new AttributeValue(model.RoomStatus) },
                    }
                }
            });
        }

        var request = new TransactWriteItemsRequest()
        {
            TransactItems = list
        };

        return Ok(await _client.TransactWriteItemsAsync(request));
    }
}
