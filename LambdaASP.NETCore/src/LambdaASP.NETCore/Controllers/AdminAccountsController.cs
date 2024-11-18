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
using Domain.Abstractions.Services;
using LambdaASP.NETCore.Services;
using static System.Net.Mime.MediaTypeNames;

namespace LambdaASP.NETCore.Controllers;

[Authorize]
[Route("api/admin-accounts")]
public class AdminAccountsController : ControllerBase
{
    private IConfiguration _config;
    AmazonDynamoDBClient _client;
    IAdminAccountService _adminAccountService;

    public AdminAccountsController(
        IConfiguration config, 
        IDBClientFactory<AmazonDynamoDBClient> factory,
        IAdminAccountService adminAccountService)
    {
        _config = config;
        _client = factory.GetClient();
        _adminAccountService = adminAccountService;
    }

    /*
    {
        "email": "apierce@travelersinn.com",
        "password": "{SHA-256 hash}" // see AdminAccounts table in DynamoDB us-east-1
    }
    */
    // SHA-256 for password
    [AllowAnonymous]
    [HttpPost("login")] // POST /api/admin-accounts/login
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

    [HttpGet("{id}")] // GET /api/admin-accounts/{id}
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ActionName(nameof(GetAsync))] // CreatedAtAction and .NET Async suffixing
    public async Task<IActionResult> GetAsync(string id)
    {
        try
        {
            return Ok(await _adminAccountService.GetAsync(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet] // GET /api/admin-accounts
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync()
    {
        return Ok(await _adminAccountService.GetAllAsync());
    }

    /* sample request body
    {
        "fullName": "Aiden Pierce",
        "email": "apierce@travelersinn.com",
        "passwordHash": "{SHA-256}"
    }
    */
    // conditional: if the email is already used, then returns BadRequest
    [HttpPost] // POST /api/admin-accounts
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> PostAsync([FromBody] CreateAccountDTO dto)
    {
        try
        {
            var account = await _adminAccountService.AddAsync(dto);
            return CreatedAtAction(nameof(GetAsync), new { id = account.AdminID }, value: account);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /* sample request body
    {
        "fullName": "Aiden Pierce",
        "email": "apierce@travelersinn.com",
        "accountStatus": "Active"
    }
    */
    [HttpPatch("{id}")] // PATCH /api/admin-accounts/{id}
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)] // when email already in use
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PatchAsync([FromRoute] string id, [FromBody] UpdateAdminAccountDTO dto)
    {
        try
        {
            await _adminAccountService.UpdateDetailsAsync(id, dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return Conflict(ex.Message);
        }

        return NoContent();
    }

    // TODO: endpoint to change password from temporary password
}
