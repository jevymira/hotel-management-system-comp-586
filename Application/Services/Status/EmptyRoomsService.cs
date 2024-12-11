using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Entities;

namespace Application.Services.Status;

public class EmptyRoomsService : IRoomStatusService
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
