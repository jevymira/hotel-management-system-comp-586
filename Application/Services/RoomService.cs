﻿using Application.Abstractions.Services;
using Domain.Abstractions.Repositories;
using Domain.Entities;
using Domain.Models;
using Domain.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using Application.Helpers.Services;
using Application.Models;
using Application.Contexts;
using Domain.Services.Recommendation;



namespace Application.Services;

/// <summary>
/// High-level interface for operations relating to rooms.
/// </summary>
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

    public async Task<Room> CreateAsync(PostRoomDTO roomDTO)
    {
        // check if RoomNumber (separate from RoomID) already exists
        if (await _roomRepository.RoomNumberExistsAsync(roomDTO.RoomNumber))
        {
            throw new ArgumentException($"Room Number {roomDTO.RoomNumber} is already in use.");
        }

        // convert images from encoding
        List<IFormFile> images = new List<IFormFile>();
        foreach (string encoded in roomDTO.ImagesBase64)
        {
            var file = Base64toFormFile(encoded);
            images.Add(file);
        }

        string id = IdGenerator.Get6CharBase62();
        // coordinate image upload service
        List<string> urls = await _imageService.UploadRoomImagesAsync(images, id);

        // assemble Room object
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

        // coordinate Room object persistence through repository
        await _roomRepository.SaveAsync(room);
        return room;
    }

    public async Task<Room> GetRoomAsync(string id)
    {
        var room = await _roomRepository.LoadRoomAsync(id);
        return room;
    }

    /// <summary>
    /// Get combination(s) of rooms that satisfy the given criteria, taking into account room availabilities.
    /// </summary>
    public async Task<RoomOptionsDTO> GetMatchingRoomsAsync(string checkInDate, string checkOutDate, int numRooms, int numGuests)
    {
        // coordinate repository to retrieve rooms by type
        RoomsByType rooms = new RoomsByType
        {
            Single = await _roomRepository.QueryByRoomTypeAsync("Single"),
            Double = await _roomRepository.QueryByRoomTypeAsync("Double"),
            Triple = await _roomRepository.QueryByRoomTypeAsync("Triple"),
            Quad = await _roomRepository.QueryByRoomTypeAsync("Quad")
        };

        // coordinate repository to retrieve availabilities for the specified dates across all room types
        RoomAvailabilities availabilities = new RoomAvailabilities
        {
            Single = rooms.Single.Count - await _reservationRepository.QueryOverlapCountAsync("Single", checkInDate, checkOutDate),
            Double = rooms.Double.Count - await _reservationRepository.QueryOverlapCountAsync("Double", checkInDate, checkOutDate),
            Triple = rooms.Triple.Count - await _reservationRepository.QueryOverlapCountAsync("Triple", checkInDate, checkOutDate),
            Quad = rooms.Quad.Count - await _reservationRepository.QueryOverlapCountAsync("Quad", checkInDate, checkOutDate),
        };

        RoomOptionsDTO roomOptions = new RoomOptionsDTO();

        // coordinate service (through Strategy context) to produce and set primary option
        RoomRecommendationServiceContext context = new RoomRecommendationServiceContext(new MonoRoomTypeService());
        var option = context.ExecuteAlgorithm(numGuests, numRooms, availabilities, rooms);
        if (option.Any())
            roomOptions.PrimaryOption = option;

        // switch services (through Strategy context) to produce and set alternative option(s)
        context.SetRecommendationStrategy(new MultiRoomTypeService());
        option = context.ExecuteAlgorithm(numGuests, numRooms, availabilities, rooms);
        if (option.Any())
            roomOptions.AlternativeOptions.Add(option);

        // switch services, again, to produce and set another alternative option
        context.SetRecommendationStrategy(new MonoRoomTypeDecrementService());
        option = context.ExecuteAlgorithm(numGuests, numRooms, availabilities, rooms);
        if (option.Any())
            roomOptions.AlternativeOptions.Add(option);

        // shuffle up the first alternative option if no option was provided for primary
        if (!roomOptions.PrimaryOption.Any() && roomOptions.AlternativeOptions.Any())
        {
            roomOptions.PrimaryOption = roomOptions.AlternativeOptions.First();
            roomOptions.AlternativeOptions.RemoveAt(0);
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

    /// <summary>
    /// Update details and overwrite images for the given room.
    /// </summary>
    public async Task UpdateAsync(string id, UpdateRoomDTO dto)
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

        List<IFormFile> images = new List<IFormFile>();
        foreach (string encoded in dto.ImagesBase64)
        {
            var file = Base64toFormFile(encoded);
            images.Add(file);
        }

        // coordinate upload of room images
        room.ImageUrls = await _imageService.UploadRoomImagesAsync(images, id);

        // coordinate the persistence of the room obect
        await _roomRepository.UpdateAsync(room);
    }

    private FormFile Base64toFormFile(string encoded)
    {
        var stream = new MemoryStream();
        var bytes = Convert.FromBase64String(encoded);

        stream.Write(bytes);
        stream.Position = 0;

        return new FormFile(stream, 0, stream.Length, "f", "fname");
    }
}
