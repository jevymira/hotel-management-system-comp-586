using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Domain.Abstractions.Repositories;
using Domain.Entities;
using Infrastructure.Abstractions.Database;
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

    public async Task<List<Reservation>> QueryConfirmedTodayAsync(string date)
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        var cfg = new DynamoDBOperationConfig
        {
            IndexName = "BookingStatus-CheckInDate-index",
            QueryFilter = new List<ScanCondition>() {
                new ScanCondition("CheckInDate", ScanOperator.Equal, date)
            }
        };
        var reservations = await context.QueryAsync<Reservation>("Confirmed", cfg).GetRemainingAsync();
        return reservations;
    }

    public async Task<int> QueryOverlapCountAsync(Reservation reservation, string bookingStatus)
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
        expressionAttributeValues.Add(":v_room_type", reservation.RoomType);
        expressionAttributeValues.Add(":v_status", bookingStatus);
        expressionAttributeValues.Add(":check_in", reservation.CheckInDate);
        expressionAttributeValues.Add(":check_out", reservation.CheckOutDate);

        var query = new QueryOperationConfig
        {
            IndexName = "RoomType-BookingStatus-index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "RoomType = :v_room_type AND BookingStatus = :v_status",
                ExpressionAttributeValues = expressionAttributeValues
            },
            FilterExpression = new Expression
            {
                ExpressionStatement = "CheckInDate < :check_out AND CheckOutDate > :check_in",
            }
        };

        var reservations = await context.FromQueryAsync<Reservation>(query).GetRemainingAsync();

        int count = 0;
        foreach (Reservation existing in reservations)
        {
            count += existing.OrderQuantity;
        }
        return count;
    }

    public async Task<int> QueryOverlapCountAsync(string roomType, string checkInDate, string checkOutDate, string bookingStatus)
    {
        DynamoDBContext context = new DynamoDBContext(_client);
        var expressionAttributeValues = new Dictionary<string, DynamoDBEntry>();
        expressionAttributeValues.Add(":v_room_type", roomType);
        expressionAttributeValues.Add(":v_status", bookingStatus);
        expressionAttributeValues.Add(":check_in", checkInDate);
        expressionAttributeValues.Add(":check_out", checkOutDate);

        var query = new QueryOperationConfig
        {
            IndexName = "RoomType-BookingStatus-index",
            KeyExpression = new Expression
            {
                ExpressionStatement = "RoomType = :v_room_type AND BookingStatus = :v_status",
                ExpressionAttributeValues = expressionAttributeValues
            },
            FilterExpression = new Expression
            {
                ExpressionStatement = "CheckInDate < :check_out AND CheckOutDate > :check_in",
            }
        };

        var reservations = await context.FromQueryAsync<Reservation>(query).GetRemainingAsync();

        int count = 0;
        foreach (Reservation existing in reservations)
        {
            count += existing.OrderQuantity;
        }
        return count;
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

    public async Task TransactWriteDueInReservations(List<Reservation> reservations)
    {
        List<TransactWriteItem> writes = new List<TransactWriteItem>();

        foreach (Reservation reservation in reservations)
        {
            writes.Add(new TransactWriteItem()
            {
                Update = new Update()
                {
                    Key = new Dictionary<string, AttributeValue>
                    {
                        { "ReservationID", new AttributeValue(reservation.ReservationID) },
                    },
                    TableName = "Reservations",
                    UpdateExpression = "SET BookingStatus = :due_in",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        { ":due_in", new AttributeValue("Due In") },
                    }
                }
            });
        }

        await _client.TransactWriteItemsAsync(new TransactWriteItemsRequest() { TransactItems = writes });
    }

}
