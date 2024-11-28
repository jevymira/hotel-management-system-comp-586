using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;

namespace Domain.Services;

public class RevertToDueInService : IRoomReservationService
{
    private readonly IRoomRepository _roomRepository;

    public RevertToDueInService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = new List<Room>();

        // from the reservation, find the rooms (if any) and change their status
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

        reservation.MakeDueIn();

        return rooms;
    }
}
