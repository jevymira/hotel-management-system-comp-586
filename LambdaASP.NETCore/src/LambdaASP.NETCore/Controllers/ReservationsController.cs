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

namespace LambdaASP.NETCore.Controllers;

[Route("api/[controller]")]
public class ReservationsController : ControllerBase
{
    AmazonDynamoDBClient _client;
    Random _random;

    public ReservationsController(IDBClientFactory<AmazonDynamoDBClient> factory)
    {
        _client = factory.GetClient();
        _random = new Random();
    }

    // GET api/reservations (start defaults to current date, end defaults to a month after)
    // GET api/reservations?start=2024-12-01
    // GET api/reservations?start=2024-12-01&end=2024-12-30
    // GET api/reservations?end=2024-12-30
    [HttpGet]
    public async Task<IActionResult> QueryByDate(string start, string end) // [FromQuery] string date)
    {
        DynamoDBContext dbContext = new DynamoDBContext(_client);
        var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
        expressionAttributeValues.Add(":v_PK", "RESERVATION");
        // expressionAttributeValues.Add(":v_StartDate", HttpContext.Request.Query["StartDate"].ToString());
        // expressionAttributeValues.Add(":v_EndDate", HttpContext.Request.Query["EndDate"].ToString());

        string date = start;

        if (start != null)
            expressionAttributeValues.Add(":v_start", date);
        else
        {
            date = DateTime.Now.ToString("yyyy-MM-dd");
            expressionAttributeValues.Add(":v_start", date);
        }
        
        if (end != null)
            expressionAttributeValues.Add(":v_end", end);
        else
            expressionAttributeValues.Add(":v_end", DateTime
                                                    .ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
                                                    .AddMonths(1).ToString("yyyy-MM-dd")
            );

        var queryOperationConfig = new QueryOperationConfig
        {
            IndexName = "PK-CheckInDate-index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "PK = :v_PK AND CheckInDate BETWEEN :v_start AND :v_end",
                ExpressionAttributeValues = expressionAttributeValues
            },
            /* FilterExpression = new Expression
            {
                ExpressionStatement = "StartDate >= :v_StartDate",
                ExpressionAttributeValues = filterAttributes
            }*/
        };
        return Ok(await dbContext
                        .FromQueryAsync<Reservation>(queryOperationConfig)
                        .GetRemainingAsync());
    }

    // PUT api/bookings
    // request Body
    // {
    //    "CheckInDate": "2024-12-01",
    //    "CheckOutDate": "2024-12-03",
    //    "NumberOfGuests": 2,
    //    "TotalPrice" : 75.00,
    //    "GuestFullName": "John Doe",
    //    "GuestEmail": "jdoe@email.com",
    //    "GuestPhoneNumber": "(555) 555-5555",
    //    "GuestDateOfBirth": "1980-01-01",
    // }
[HttpPut()]
    public async Task<string> CreateReservation([FromBody] Reservation reservation)
    {
        string guid = $"RESERVATION#{System.Guid.NewGuid().ToString("D").ToUpper()}";
        var item = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue { S = "RESERVATION" },
            ["SK"] = new AttributeValue { S = guid },
            ["RoomID"] = new AttributeValue { S = "" },
            ["CheckInDate"] = new AttributeValue { S = reservation.CheckInDate },
            ["CheckOutDate"] = new AttributeValue { S = reservation.CheckOutDate },
            ["NumberOfGuests"] = new AttributeValue { N = reservation.NumberOfGuests.ToString() },
            ["TotalPrice"] = new AttributeValue { N = reservation.TotalPrice.ToString() },
            ["BookingStatus"] = new AttributeValue { S = "Confirmed" },
            ["GuestFullName"] = new AttributeValue { S = reservation.GuestFullName },
            ["GuestEmail"] = new AttributeValue { S = reservation.GuestEmail },
            ["GuestPhoneNumber"] = new AttributeValue { S = reservation.GuestPhoneNumber },
            ["GuestDateOfBirth"] = new AttributeValue { S = reservation.GuestDateOfBirth },
            ["UpdatedBy"] = new AttributeValue { S = String.Empty },
            // ["ConfirmationNumber"] = new AttributeValue
            //     { S = _random.Next(10000000,99999999).ToString().PadLeft(8, '0')},
        };
        var putRequest = new PutItemRequest
        {
            TableName = "Hotel",
            Item = item
        };

        await _client.PutItemAsync(putRequest);
        // return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        return guid.Substring(12, 8); // confirmation number a substring of the guid
        // begins_with query condition allows searching
        // by name & confirmation number combination
    }
}
