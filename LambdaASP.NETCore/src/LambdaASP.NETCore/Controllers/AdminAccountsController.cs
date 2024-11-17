using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Abstractions;
using Microsoft.AspNetCore.Authorization;
using Domain.Entities;
using Domain.Models;

namespace LambdaASP.NETCore.Controllers;

[Authorize]
[Route("api/admin-accounts")]
public class AdminAccountsController : ControllerBase
{
    private IConfiguration _config;
    AmazonDynamoDBClient _client;

    public AdminAccountsController(IConfiguration config, IDBClientFactory<AmazonDynamoDBClient> factory)
    {
        _config = config;
        _client = factory.GetClient();
    }

    /*
    {
        "email": "apierce@travelersinn.com",
        "password": "{SHA-256 hash}" // see AdminAccounts table in DynamoDB us-east-1
    }
    */
    // SHA-256 for password
    [AllowAnonymous]
    [HttpPost("login")] // POST api/admin-accounts/login
    public async Task<IActionResult> Post([FromBody] LoginRequest loginRequest)
    {
        AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        DynamoDBContext context = new DynamoDBContext(client);

        // check for account with matching Email and Password AND which is Active
        var request = new QueryRequest
        {
            TableName = "AdminAccounts",
            IndexName = "Email-PasswordHash-index",
            KeyConditionExpression = "Email = :email AND PasswordHash = :password_hash",
            FilterExpression = "AccountStatus = :status",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":email", new AttributeValue(loginRequest.Email) },
                { ":password_hash", new AttributeValue(loginRequest.Password) },
                { ":status", new AttributeValue("Active") }
            },
        };

        var data = await client.QueryAsync(request);
        if (data.Count == 0) // if query returns no account with with email+password combination
            return StatusCode(401);
        else
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var Sectoken = new JwtSecurityToken(_config["Jwt:Issuer"],
              _config["Jwt:Issuer"],
              null,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

            return Ok(token);
        }
    }

    // Scan viable because there is a singular Item type in the table
    [HttpGet] // GET api/admin-accounts
    public async Task<IActionResult> Get()
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        return Ok(await context.ScanAsync<AdminAccount>(default).GetRemainingAsync());
    }

    /* sample request body
    {
        "name": "Aiden Pierce",
        "email": "apierce@travelersinn.com"
    }
    */
    // conditional: if the email is already used, then returns BadRequest
    [HttpPut]
    public async Task<IActionResult> Put([FromBody] CreateAccountDTO model)
    {
        var request = new QueryRequest
        {
            TableName = "AdminAccounts",
            IndexName = "Email-PasswordHash-index",
            KeyConditionExpression = "Email = :email",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":email", new AttributeValue(model.Email) }
            },
        };

        var data = await _client.QueryAsync(request);
        if (data.Count >= 1) // if query returns an account with this email
            return BadRequest("Email already used.");
        else
        {
            Random random = new Random();
            string id = random.Next(100000000, 2147483647).ToString().PadLeft(10, '0');
            string temp = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes("temp")));

            var item = new Dictionary<string, AttributeValue>
            {
                { "AdminID", new AttributeValue(id) },
                { "FullName", new AttributeValue(model.Name) },
                { "Email", new AttributeValue(model.Email) },
                { "PasswordHash", new AttributeValue(temp) },
                { "AccountStatus", new AttributeValue("Active") }
            };

            var putRequest = new PutItemRequest
            {
                TableName = "AdminAccounts",
                Item = item
            };

            return Ok(await _client.PutItemAsync(putRequest));
        }
    }

    // TODO: endpoint to change password from temporary password
}
