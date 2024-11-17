using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Domain.Abstractions.Services;

public interface IRoomService
{
    public Task<Room> CreateAsync(PostRoomDTO roomDTO, List<IFormFile> images);
    //public Task RoomNumberTakenAsync(string roomNumber);
}
