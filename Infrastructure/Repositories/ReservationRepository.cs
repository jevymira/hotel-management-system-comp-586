using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain.Abstractions.Repositories;
using Domain.Entities;
using Domain.Models;
using Infrastructure.Abstractions.Database;
using System.Data.Common;
using System.Transactions;

namespace Infrastructure.Repositories;

public class ReservationRepository : IReservationRepository
{
    private readonly AmazonDynamoDBClient _client;

    public ReservationRepository(IDBClientFactory<AmazonDynamoDBClient> factory)
    {
        _client = factory.GetClient();
    }

    public async Task SaveAsync(Reservation reservation)
    {
        var context = new DynamoDBContext(_client);
        await context.SaveAsync(reservation);
;    }

    public async Task<Reservation> LoadReservationAsync(string id)
    {
        var context = new DynamoDBContext(_client);
        return await context.LoadAsync<Reservation>(id);
    }

    public async Task<List<Reservation>> QueryByNameAsync(string name)
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

        return await context.FromQueryAsync<Reservation>(queryOperationConfig).GetRemainingAsync();
    }

    public async Task<List<Reservation>> QueryDueInAsync()
    {
        DynamoDBContext context = new DynamoDBContext(_client);
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

        return await context.FromQueryAsync<Reservation>(query).GetRemainingAsync();
    }
    public async Task<List<Reservation>> QueryCheckedInAsync()
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
        expressionAttributeValues.Add(":v_status", "Checked In");

        var query = new QueryOperationConfig
        {
            IndexName = "BookingStatus-CheckOutDate-index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "BookingStatus = :v_status",
                ExpressionAttributeValues = expressionAttributeValues
            }
        };

        return await context.FromQueryAsync<Reservation>(query).GetRemainingAsync();
    }

    public async Task<List<Reservation>> QueryCheckedOutAsync(string date)
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
        expressionAttributeValues.Add(":v_status", "Checked Out");
        expressionAttributeValues.Add(":v_date", date);

        var query = new QueryOperationConfig
        {
            IndexName = "BookingStatus-CheckOutDate-index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "BookingStatus = :v_status AND CheckOutDate = :v_date",
                ExpressionAttributeValues = expressionAttributeValues
            }
        };

        return await context.FromQueryAsync<Reservation>(query).GetRemainingAsync();
    }

    public async Task<List<Reservation>> QueryConfirmedAsync(string date)
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
        expressionAttributeValues.Add(":v_status", "Confirmed");
        expressionAttributeValues.Add(":v_date", date);

        var query = new QueryOperationConfig
        {
            IndexName = "BookingStatus-CheckInDate-index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "BookingStatus = :v_status AND CheckInDate >= :v_date",
                ExpressionAttributeValues = expressionAttributeValues
            }
        };

        return await context.FromQueryAsync<Reservation>(query).GetRemainingAsync();
    }

    public async Task TransactWriteCheckInAsync(string id, string status, string by, List<string> roomIDs)
    {
        List<TransactWriteItem> writes = new List<TransactWriteItem>()
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
                                            "RoomIDs = :rooms, " +
                                            "UpdatedBy = :updated_by",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":status", new AttributeValue(status) },
                        { ":rooms", new AttributeValue(roomIDs) },
                        { ":updated_by", new AttributeValue(by) }
                    },
                }
            },
        };

        foreach (string roomID in roomIDs)
        {
            writes.Add(GetRoomStatusUpdateWriteItem(roomID, status, by));
        }

        var request = new TransactWriteItemsRequest()
        {
            TransactItems = writes
        };

        await _client.TransactWriteItemsAsync(request);
    }

    public async Task TransactWriteCheckOutAsync(string id, string status, string by, List<string> roomIDs)
    {
        List<TransactWriteItem> writes = new List<TransactWriteItem>()
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
                                            "UpdatedBy = :updated_by " +
                                       "REMOVE RoomIDs",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":status", new AttributeValue(status) },
                        { ":updated_by", new AttributeValue(by) }
                    },
                }
            },
        };

        foreach (string roomID in roomIDs)
        {
            writes.Add(GetRoomStatusUpdateWriteItem(roomID, status, by));
        }

        var request = new TransactWriteItemsRequest()
        {
            TransactItems = writes
        };

        await _client.TransactWriteItemsAsync(request);
    }

    private TransactWriteItem GetRoomStatusUpdateWriteItem(string roomID, string roomStatus, string updatedBy)
    {
        return new TransactWriteItem()
        {
            Update = new Update()
            {
                TableName = "Rooms",
                Key = new Dictionary<string, AttributeValue>
                {
                    { "RoomID", new AttributeValue(roomID) }
                },
                UpdateExpression = "SET #s = :status, " +
                                        "UpdatedBy = :updated_by",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    { "#s", "Status" }, // alias, status is a reserved word
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":status", new AttributeValue(roomStatus) },
                    { ":updated_by", new AttributeValue(updatedBy) }
                }
            }
        };
    }

    public async Task TransactWriteRoomReservationAsync(Reservation reservation, List<Room> rooms)
    {
        List<TransactWriteItem> writes = new List<TransactWriteItem>()
        {
            new TransactWriteItem()
            {
                Put = new Put()
                {
                    TableName = "Reservations",
                    Item = new Dictionary<string, AttributeValue>
                    {
                        {"ReservationID", new AttributeValue(reservation.ReservationID)},
                        {"RoomType", new AttributeValue(reservation.RoomType)},
                        {"OrderQuantity", new AttributeValue(reservation.OrderQuantity.ToString())},
                        {"CheckInDate", new AttributeValue(reservation.CheckInDate)},
                        {"CheckOutDate", new AttributeValue(reservation.CheckOutDate)},
                        {"NumberOfGuests", new AttributeValue(reservation.NumberOfGuests.ToString())},
                        {"TotalPrice", new AttributeValue(reservation.TotalPrice.ToString())},
                        {"BookingStatus", new AttributeValue(reservation.BookingStatus)},
                        {"GuestFullName", new AttributeValue(reservation.GuestFullName)},
                        {"GuestEmail", new AttributeValue(reservation.GuestEmail)},
                        {"GuestPhoneNumber", new AttributeValue(reservation.GuestPhoneNumber)},
                        {"GuestDateOfBirth", new AttributeValue(reservation.GuestDateOfBirth)},
                        {"UpdatedBy", new AttributeValue(reservation.UpdatedBy)}
                    }
                }
            }
        };

        if (reservation.RoomIDs.Count > 0) // null sets not allowed
        {
            writes.First().Put.Item.Add("RoomIDs", new AttributeValue(reservation.RoomIDs));
        }

        foreach (Room room in rooms)
        {
            writes.Add(GetRoomWriteItem(room));
        }

        var request = new TransactWriteItemsRequest()
        {
            TransactItems = writes
        };

        try 
        {
            var resp = await _client.TransactWriteItemsAsync(request);
        }
        catch (TransactionCanceledException) // DynamoDBv2
        {
            throw new TransactionException("Update failed, try again.");
        }
    }

    private TransactWriteItem GetRoomWriteItem(Room room)
    {
        return new TransactWriteItem()
        {
            Put = new Put()
            {
                TableName = "Rooms",
                Item = new Dictionary<string, AttributeValue>
                {
                    {"RoomID", new AttributeValue(room.RoomID)},
                    {"RoomTypeID", new AttributeValue(room.RoomTypeID)},
                    {"RoomNumber", new AttributeValue(room.RoomNumber)},
                    {"PricePerNight", new AttributeValue(room.PricePerNight.ToString())},
                    {"MaxOccupancy", new AttributeValue(room.MaxOccupancy.ToString())},
                    {"Status", new AttributeValue(room.Status)},
                    {"RoomSize", new AttributeValue(room.RoomSize)},
                    {"ImageUrls", new AttributeValue(room.ImageUrls)},
                    {"UpdatedBy", new AttributeValue(room.UpdatedBy)}
                }
            }
        };
    }

}
