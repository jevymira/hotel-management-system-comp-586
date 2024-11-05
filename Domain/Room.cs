﻿using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;

namespace Domain;

[DynamoDBTable("Hotel")]
public class Room
{
    [DynamoDBHashKey]
    public string? PK { get; set; } // EntityType (ROOM)

    [DynamoDBRangeKey]
    public string? SK { get; set; } // GUID (ROOM#GUID)

    public string? RoomTypeID { get; set; }

    public string? RoomNumber { get; set; }

    public decimal? PricePerNight { get; set; }

    public int? MaxOccupancy { get; set; }

    public string? Status { get; set; }

    public string? RoomSize { get; set; }

    public List<string>? ImageUrls { get; set; }

    public string? UpdatedBy { get; set; }
}
