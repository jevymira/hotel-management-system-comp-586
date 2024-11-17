using Domain.Entities;
using Domain.Models;

namespace Domain.Abstractions.Repositories;

public interface IRoomRepository
{
    public Task<Room> SaveAsync(Room room);
    public Task<Room> LoadAsync(string id);
    public Task<List<Room>> ScanAsync();
    public Task UpdateAsync(string id, UpdateRoomDTO roomDTO, List<string> urls);
    public Task<bool> RoomIdExistsAsync(string id);
    public Task<bool> RoomNumberExistsAsync(string num);
    public Task<bool> RoomNumberExistsElsewhereAsync(string num, string id);
}
