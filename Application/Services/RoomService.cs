using Application.Abstractions.Services;
using Domain;
using Domain.Abstractions.Repositories;
using Domain.Entities;
using Domain.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using Application.Helpers.Services;
using Application.Models;


namespace Application.Services;

public class RoomService : IRoomService
{
    IRoomRepository _repository;
    IImageService _imageService;

    public RoomService(IRoomRepository repository, IImageService imageService)
    {
        _repository = repository;
        _imageService = imageService;
    }

    public async Task<Room?> CreateAsync(PostRoomDTO roomDTO, List<IFormFile> images)
    {
        // check if RoomNumber (separate from RoomID) already exists
        if (await _repository.RoomNumberExistsAsync(roomDTO.RoomNumber))
        {
            return null;
        }

        string id = IdGenerator.Get6CharBase62();
        List<string> urls = await _imageService.UploadRoomImagesAsync(images, id);

        Room room = new Room
        {
            RoomID = id,
            RoomTypeID = roomDTO.RoomTypeID,
            RoomNumber = roomDTO.RoomNumber,
            PricePerNight = roomDTO.PricePerNight,
            MaxOccupancy = roomDTO.MaxOccupancy,
            Status = "Empty",
            RoomSize = "20 m^2 / 215 ft^2",
            ImageUrls = urls,
            UpdatedBy = String.Empty
        };

        await _repository.SaveAsync(room);
        return room;
    }

    public async Task<Room> GetRoomAsync(string id)
    {
        var room = await _repository.LoadRoomAsync(id);
        return room;
    }

    public async Task<List<Room>> GetAllAsync()
    {
        return await _repository.ScanAsync();
    }

    public async Task<List<Room>> GetEmptyRoomsByType(string type)
    {
        return await _repository.QueryEmptyByRoomTypeAsync(type);
    }

    public async Task<Result<string>> UpdateAsync(string id, UpdateRoomDTO dto, List<IFormFile> images)
    {
        if (!(await _repository.RoomIdExistsAsync(id)))
            return new Result<string>(new Error("NotFound"));
            // throw new KeyNotFoundException($"No room exists with Room ID {id}.");

        // check if RoomNumber (separate from RoomID) is unique
        if (await _repository.RoomNumberExistsElsewhereAsync(dto.RoomNumber, id))
            return new Result<string>(new Error($"Conflict"));
            // throw new ArgumentException($"Room Number {roomDTO.RoomNumber} is already in use with another room.");

        List<string> urls = await _imageService.UploadRoomImagesAsync(images, id);
        await _repository.UpdateAsync(id, dto.RoomTypeID, dto.PricePerNight, 
            dto.MaxOccupancy, dto.RoomNumber, urls, dto.UpdatedBy);
        return new Result<string>("Updated successfully.");
    }
}
