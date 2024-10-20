using Abstractions;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Text.Json;
using System.Globalization;

namespace LambdaASP.NETCore.Controllers;

[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    AmazonDynamoDBClient _client;
    Random _random;

    public BookingsController(IDBClientFactory<AmazonDynamoDBClient> factory)
    {
        _client = factory.GetClient();
        _random = new Random();
    }

    // GET api/bookings (start defaults to current date, end default to a month after)
    // GET api/bookings?start=2024-12-01
    // GET api/bookings?start=2024-12-01&end=2024-12-30
    // GET api/bookings?end=2024-12-30
    [HttpGet]
    public async Task<IActionResult> QueryByDate(string start, string end) // [FromQuery] string date)
    {
        DynamoDBContext dbContext = new DynamoDBContext(_client);
        var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
        expressionAttributeValues.Add(":v_PK", "BOOKING");
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
            IndexName = "PK-EndDate-index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "PK = :v_PK AND EndDate BETWEEN :v_start AND :v_end",
                ExpressionAttributeValues = expressionAttributeValues
            },
            /* FilterExpression = new Expression
            {
                ExpressionStatement = "StartDate >= :v_StartDate",
                ExpressionAttributeValues = filterAttributes
            }*/
        };
        return Ok(await dbContext
                        .FromQueryAsync<Booking>(queryOperationConfig)
                        .GetRemainingAsync());
    }

    // PUT api/bookings
    // request Body
    // {
    //     "StartDate": "2024-12-01",
    //     "EndDate": "2024-12-03",
    //     "RoomType": "Double",
    //     "Name": "John Doe",
    //     "Email": "jdoe@email.com",
    //     "PhoneNumber": "(555) 555-5555"
    // }
    [HttpPut()]
    public async Task<bool> CreateBooking([FromBody] Booking newBooking)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["PK"] = new AttributeValue { S = "BOOKING" },
            ["SK"] = new AttributeValue
                { S = $"BOOKING#{System.Guid.NewGuid().ToString("D").ToUpper()}" },
            ["RoomType"] = new AttributeValue { S = newBooking.RoomType },
            ["StartDate"] = new AttributeValue { S = newBooking.StartDate },
            ["EndDate"] = new AttributeValue { S = newBooking.EndDate },
            ["Name"] = new AttributeValue { S = newBooking.Name },
            ["Email"] = new AttributeValue { S = newBooking.Email },
            ["PhoneNumber"] = new AttributeValue { S = newBooking.PhoneNumber },
            ["ConfirmationNumber"] = new AttributeValue
                { S = _random.Next(10000000,99999999).ToString().PadLeft(8, '0')},
            ["Status"] = new AttributeValue { S = "Confirmed" },
        };
        var putRequest = new PutItemRequest
        {
            TableName = "Hotel",
            Item = item
        };

        var response = await _client.PutItemAsync(putRequest);
        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }
}
