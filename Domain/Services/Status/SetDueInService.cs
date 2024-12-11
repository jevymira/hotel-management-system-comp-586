using Domain.Abstractions.Services;
using Domain.Entities;

namespace Domain.Services.Status;

/// <summary>
/// Concrete Strategy for a service that handles updating the status of 
/// a reservation and those of its assigned rooms.
/// </summary>
/// <remarks>
/// The Refined Abstraction in a Bridge pattern with Implementor IRoomStatusService.
/// </remarks>
public class SetDueInService : RoomReservationService
{
    private readonly IRoomStatusService _roomsStatusService;

    public SetDueInService(IRoomStatusService roomsStatusService) : base(roomsStatusService)
    {
        _roomsStatusService = roomsStatusService;
    }

    /// <summary>
    /// Marks the reservation as "Due In" and unassigns rooms (if any),
    /// doing both together to ensure consistency between statuses.
    /// </summary>
    /// <param name="reservation">Reservation to be updated.</param>
    /// <param name="roomNumbers">Room Numbers of rooms to be updated.</param>
    /// <returns>Updated Room(s).</returns>
    public override async Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = await _roomsStatusService.UpdateStatuses(reservation, roomNumbers);

        reservation.MarkDueIn(); // clears room assignment in reservation

        return rooms;
    }
}
