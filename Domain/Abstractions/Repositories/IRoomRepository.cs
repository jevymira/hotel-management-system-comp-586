using Domain.Entities;

namespace Domain.Abstractions.Repositories;

/// <summary>
/// Encapsulates the logic for the retrieval/persistence of hotel rooms.
/// </summary>
public interface IRoomRepository
{
    public Task<Room> SaveAsync(Room room);
    public Task<Room> LoadRoomAsync(string id);

    /// <summary>
    /// Scan, instead of query, the table. Possible because the Rooms
    /// table consist of a single entity type.
    /// </summary>
    public Task<List<Room>> ScanAsync();
    public Task UpdateAsync(Room room);
    public Task<bool> RoomIdExistsAsync(string id);
    public Task<bool> RoomNumberExistsAsync(string num);

    /// <summary>
    /// Query whether room number already taken outside a given room entity,
    /// specified by its ID.
    /// </summary>
    /// <param name="num">Room Number</param>
    /// <param name="id">Room ID, for room to ignore</param>
    public Task<bool> RoomNumberExistsElsewhereAsync(string num, string id);
    public Task<Room?> QueryByRoomNumberAsync(string num);

    /// <summary>
    /// Query all empty rooms of a specified type.
    /// </summary>
    /// <param name="roomType">Room Type to be queried for.</param>
    public Task<List<Room>> QueryEmptyByRoomTypeAsync(string roomType);
    public Task<List<Room>> QueryByRoomTypeAsync(string roomType);
}
