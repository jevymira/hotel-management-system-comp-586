using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;

namespace Domain.Services.Status;

/// <summary>
/// Service that handles status updates to the assigned rooms of a reservation.
/// </summary>
/// <remarks>
/// The Concrete Implementor in a Bridge pattern with Abstraction IRoomReservationService.
/// </remarks>
public class EmptyRoomsService : IRoomStatusService
{
    private readonly IRoomRepository _roomRepository;

    public EmptyRoomsService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    /// <summary>
    /// Marks the rooms "Empty" which correspond to the provided room numbers.
    /// </summary>
    /// <param name="reservation">Reservation with the rooms.</param>
    /// <param name="roomNumbers">Room Numbers of rooms to be updated.</param>
    /// <returns>Updated Room(s).</returns>
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
