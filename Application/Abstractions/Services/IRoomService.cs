﻿using Application.Entities;
using Application.Models;
using Microsoft.AspNetCore.Http;

namespace Application.Abstractions.Services;

public interface IRoomService
{
    public Task<Room> CreateAsync(PostRoomDTO roomDTO, List<IFormFile> images);
    public Task<Room> GetRoomAsync(string id);
    public Task<RoomOptionsDTO> GetMatchingRoomsAsync(string checkInDate, string checkOutDate, int numRooms, int numGuests);
    public Task<List<Room>> GetAllAsync();
    public Task<List<Room>> GetEmptyRoomsByType(string type);
    public Task UpdateAsync(string id, UpdateRoomDTO roomDTO, List<IFormFile> images);
}
