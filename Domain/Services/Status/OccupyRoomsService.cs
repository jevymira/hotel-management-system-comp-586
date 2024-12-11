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
public class OccupyRoomsService : IRoomStatusService
{
    private readonly IRoomRepository _roomRepository;

    public OccupyRoomsService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    /// <summary>
    /// Marks the rooms "Occupied" which correspond to the specified room numbers,
    /// if not already occupied by another reservation.
    /// </summary>
    /// <param name="reservation">Reservation with the rooms.</param>
    /// <param name="roomNumbers">Room Numbers of rooms to be updated.</param>
    /// <returns>Updated Room(s).</returns>
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
