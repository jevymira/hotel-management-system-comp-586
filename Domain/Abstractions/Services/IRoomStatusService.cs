using Domain.Entities;

namespace Domain.Abstractions.Services;

/// <summary>
/// Common interface for services to handle status
/// updates to a reservation and its assigned rooms.
/// </summary>
/// <remarks>
/// The Implementor in a Bridge pattern with Abstraction IRoomReservationService.
/// </remarks>
public interface IRoomStatusService
{
    /// <summary>
    /// Updates the status of the specified reservations and the statuses of 
    /// its assigned rooms together to make sure of their consistency.
    /// </summary>
    /// <param name="reservation">Reservation to be updated.</param>
    /// <param name="roomNumbers">Room Numbers of rooms to be updated.</param>
    /// <returns>Updated rooms.</returns>
    public Task<List<Room>> UpdateStatuses(Reservation reservation, List<string> roomNumbers);
}
