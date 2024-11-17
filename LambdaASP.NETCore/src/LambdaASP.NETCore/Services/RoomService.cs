using Abstractions;
using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;
using Domain.Models;

namespace LambdaASP.NETCore.Services;

public class RoomService : IRoomService
{
    IRoomRepository _repository;
    IImageService _imageService;

    public RoomService(IRoomRepository repository, IImageService imageService)
    {
        _repository = repository;
        _imageService = imageService;
    }
    public async Task<Room> CreateAsync(PostRoomDTO roomDTO, List<IFormFile> images)
    {
        if (await _repository.RoomNumberExistsAsync(roomDTO.RoomNumber))
        {
            throw new ArgumentException($"Room Number {roomDTO.RoomNumber} is already in use.");
        }

        string id = IdGenerator.Get6CharBase62();
        List<string> urls = await _imageService.UploadImagesAsync(images, id);

        Room room = new Room
        {
            RoomID = id,
            RoomTypeID = roomDTO.RoomType,
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

    //public Task RoomNumberTakenAsync(string roomNumber)
    //{
    //    return _repository.RoomNumberExistsAsync(roomNumber);
    //}
}
