using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain.Abstractions.Repositories;
using Domain.Entities;
using Domain.Models;
using Infrastructure.Abstractions.Database;

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
}
