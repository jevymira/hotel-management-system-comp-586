﻿using Amazon.DynamoDBv2.DataModel;

namespace Domain.Entities;

[DynamoDBTable("AdminAccounts")]
public class AdminAccount
{
    [DynamoDBHashKey]
    public required string AdminID { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string AccountStatus { get; set; }
}
