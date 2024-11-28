using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;

namespace Domain.Services;

public class CheckInService : IRoomReservationService
{
    private readonly IRoomRepository _roomRepository;

    public CheckInService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = new List<Room>();

        // from the room numbers provided, find the rooms and change their status
        foreach (string roomNumber in roomNumbers)
        {
            Room? room = await _roomRepository.QueryEmptyByRoomNumberAsync(roomNumber);
            if (room == null)
            {
                throw new ArgumentException("One or more provided room numbers are non-existent or occupied");
            }
            room.MarkOccupied();
            room.UpdatedBy = reservation.UpdatedBy;
            rooms.Add(room);
        }

        reservation.CheckIn(rooms);

        return rooms;
    }
}
