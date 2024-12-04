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
    private readonly IRoomRepository _roomRepository;
    private readonly IImageService _imageService;
    private readonly IReservationRepository _reservationRepository;

    public RoomService(
        IRoomRepository repository, 
        IImageService imageService, 
        IReservationRepository reservationRepository)
    {
        _roomRepository = repository;
        _imageService = imageService;
        _reservationRepository = reservationRepository;
    }

    public async Task<Room> CreateAsync(PostRoomDTO roomDTO, List<IFormFile> images)
    {
        // check if RoomNumber (separate from RoomID) already exists
        if (await _roomRepository.RoomNumberExistsAsync(roomDTO.RoomNumber))
        {
            throw new ArgumentException($"Room Number {roomDTO.RoomNumber} is already in use.");
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
            RoomSize = "20 m^2 / 215 ft^2",
            ImageUrls = urls,
            UpdatedBy = string.Empty
        };
        room.MarkEmpty();

        await _roomRepository.SaveAsync(room);
        return room;
    }

    public async Task<Room> GetRoomAsync(string id)
    {
        var room = await _roomRepository.LoadRoomAsync(id);
        return room;
    }

    public async Task<RoomOptionsDTO> GetMatchingRoomsAsync(string checkInDate, string checkOutDate, int numRooms, int numGuests)
    {
        RoomOptionsDTO roomOptions = new RoomOptionsDTO();

        int perRoom = (int)Math.Ceiling((double)numGuests / (double)numRooms);

        switch (perRoom)
        {
            case 1: 
                roomOptions.primaryOption = new RoomOptionDTO { Type = "Single", Quantity = numRooms };
                break;
            case 2:
                roomOptions.primaryOption = new RoomOptionDTO { Type = "Double", Quantity = numRooms };
                break;
            case 3:
                roomOptions.primaryOption = new RoomOptionDTO { Type = "Triple", Quantity = numRooms };
                break;
            case 4:
            default: // TODO: handle other cases for which perRoom is significantly larger than 4
                roomOptions.primaryOption = new RoomOptionDTO { Type = "Quad", Quantity = numRooms };
                break;
        }

        return roomOptions;
    }

    public async Task<List<Room>> GetAllAsync()
    {
        return await _roomRepository.ScanAsync();
    }

    public async Task<List<Room>> GetEmptyRoomsByType(string type)
    {
        return await _roomRepository.QueryEmptyByRoomTypeAsync(type);
    }

    public async Task UpdateAsync(string id, UpdateRoomDTO dto, List<IFormFile> images)
    {
        // load in the room to be updated
        Room? room = await _roomRepository.LoadRoomAsync(id)
            ?? throw new KeyNotFoundException($"No room exists with Room ID {id}.");

        // check if pending RoomNumber (separate from RoomID) is unique
        if (await _roomRepository.RoomNumberExistsElsewhereAsync(dto.RoomNumber, id))
            throw new ArgumentException($"Room Number {dto.RoomNumber} is already in use with another room.");

        // make changes to room object
        room.RoomTypeID = dto.RoomTypeID;
        room.PricePerNight = dto.PricePerNight;
        room.MaxOccupancy = dto.MaxOccupancy;
        room.RoomNumber = dto.RoomNumber;
        room.UpdatedBy = dto.UpdatedBy;

        // coordinate upload of room images
        room.ImageUrls = await _imageService.UploadRoomImagesAsync(images, id);

        // coordinate the persistence of the room obect
        await _roomRepository.UpdateAsync(room);
    }
}
