using Application.Abstractions.Services;
using Microsoft.AspNetCore.Http;
using Application.Helpers.Services;
using Application.Models;
using Application.Contexts;
using Application.Abstractions.Repositories;
using Application.Entities;
using Application.Services.Recommendation;


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

    public RoomRecommendationServiceContext strategyContext
    {
        get => default;
        set
        {
        }
    }

    public Room room
    {
        get => default;
        set
        {
        }
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
