using Domain.Entities;
using Domain.Models;

namespace Domain.Abstractions.Repositories;

public interface IRoomRepository
{
    public Task<Room> SaveAsync(Room room);
    public Task<bool> RoomNumberExistsAsync(string id);
}
