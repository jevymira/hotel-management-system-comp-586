using Abstractions;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using LambdaASP.NETCore.Models;

[Route("api/[controller]")]
public class AdminAccountsController : ControllerBase
{
    [HttpPost("authenticate")]
    public IActionResult Authenticate(AuthenticateRequest)
    {
        
    }
}
