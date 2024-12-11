using Application.Models;
using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Abstractions.Services;

/// <summary>
/// High-level interface for operations relating to rooms.
/// </summary>
public interface IRoomService
{
    public Task<Room> CreateAsync(PostRoomDTO roomDTO, List<IFormFile> images);
    public Task<Room> GetRoomAsync(string id);

    /// <summary>
    /// Get combination(s) of rooms that satisfy the given criteria, taking into
    /// account room availabilities.
    /// </summary>
    public Task<RoomOptionsDTO> GetMatchingRoomsAsync(
        string checkInDate, string checkOutDate, int numRooms, int numGuests
    );
    public Task<List<Room>> GetAllAsync();
    public Task<List<Room>> GetEmptyRoomsByType(string type);

    /// <summary>
    /// Update details and overwrite images for the given room.
    /// </summary>
    public Task UpdateAsync(string id, UpdateRoomDTO roomDTO, List<IFormFile> images);
}
