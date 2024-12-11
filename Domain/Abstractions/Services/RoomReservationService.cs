using Domain.Entities;

namespace Domain.Abstractions.Services;

/// <summary>
/// Common Strategy interface for services to handle 
/// updates to a reservation and its room assignments.
/// </summary>
/// <remarks>
/// The Abstraction in a Bridge pattern with Implementor IRoomStatusService.
/// </remarks>
public abstract class RoomReservationService
{
    private readonly IRoomStatusService _roomsStatusService;

    public RoomReservationService(IRoomStatusService roomsStatusService)
    {
        _roomsStatusService = roomsStatusService;
    }

    /// <summary>
    /// Processes the reservation and provided roomNumbers together
    /// to ensure their consistency when updating their attributes.
    /// </summary>
    /// <param name="reservation">Reservation to be updated.</param>
    /// <param name="roomNumbers">Room Numbers of rooms to be updated.</param>
    /// <returns>Updated Room(s).</returns>
    public abstract Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers);
}
