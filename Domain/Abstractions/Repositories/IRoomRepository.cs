using Domain.Entities;

namespace Domain.Abstractions.Repositories;

public interface IRoomRepository
{
    public Task<Room> SaveAsync(Room room);
    public Task<Room> LoadRoomAsync(string id);
    public Task<List<Room>> ScanAsync();
    public Task UpdateAsync(Room room);
    public Task<bool> RoomIdExistsAsync(string id);
    public Task<bool> RoomNumberExistsAsync(string num);
    public Task<bool> RoomNumberExistsElsewhereAsync(string num, string id);
    public Task<Room?> QueryByRoomNumberAsync(string num);
    public Task<List<Room>> QueryEmptyByRoomTypeAsync(string roomType);
}
