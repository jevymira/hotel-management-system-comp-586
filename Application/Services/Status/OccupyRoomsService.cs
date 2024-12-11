using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Entities;

namespace Application.Services.Status;

public class OccupyRoomsService : IRoomStatusService
{
    private readonly IRoomRepository _roomRepository;

    public OccupyRoomsService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }
    public async Task<List<Room>> UpdateStatuses(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = new List<Room>();

        // from the room numbers provided, find the rooms
        foreach (string roomNumber in roomNumbers)
        {
            Room? room = await _roomRepository.QueryByRoomNumberAsync(roomNumber)
                ?? throw new ArgumentException("At least one provided room number is non-existent");

            if (room.IsOccupied() && !reservation.RoomIDs.Contains(room.RoomID))
                throw new ArgumentException($"Room number {roomNumber} is already assigned to another reservation.");

            room.MarkOccupied();
            room.UpdatedBy = reservation.UpdatedBy;

            rooms.Add(room);
        }

        return rooms;
    }
}
