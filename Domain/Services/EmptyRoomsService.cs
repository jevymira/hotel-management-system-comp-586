using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;

namespace Domain.Services;

public class EmptyRoomsService : IRoomsStatusService
{
    private readonly IRoomRepository _roomRepository;

    public EmptyRoomsService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<List<Room>> UpdateStatuses(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = new List<Room>();

        // from the reservation, find the rooms
        foreach (string roomID in reservation.RoomIDs)
        {
            Room? room = await _roomRepository.LoadRoomAsync(roomID);
            if (room != null)
            {
                room.MarkEmpty();
                room.UpdatedBy = reservation.UpdatedBy;

                rooms.Add(room);
            }
        }

        return rooms;
    }
}
