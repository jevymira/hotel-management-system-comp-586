using Application.Models;
using Domain;
using Domain.Entities;
using Domain.Models;
using Microsoft.AspNetCore.Http;

namespace Application.Abstractions.Services;

public interface IRoomService
{
    public Task<Room> CreateAsync(PostRoomDTO roomDTO, List<IFormFile> images);
    public Task<Room> GetRoomAsync(string id);
    public Task<List<Room>> GetAllAsync();
    public Task<List<Room>> GetEmptyRoomsByType(string type);
    public Task UpdateAsync(string id, UpdateRoomDTO roomDTO, List<IFormFile> images);
}
