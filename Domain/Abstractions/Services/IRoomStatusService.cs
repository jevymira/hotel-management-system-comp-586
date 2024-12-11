using Domain.Entities;

namespace Domain.Abstractions.Services;

/// <summary>
/// Common interface for services to handle status updates to 
/// the assigned rooms of a reservation.
/// </summary>
/// <remarks>
/// The Implementor in a Bridge pattern with Abstraction IRoomReservationService.
/// </remarks>
public interface IRoomStatusService
{
    /// <summary>
    /// Updates the statuses of the rooms corresponding to the provided room numbers.
    /// </summary>
    /// <param name="reservation">Reservation with the rooms.</param>
    /// <param name="roomNumbers">Room Numbers of rooms to be updated.</param>
    /// <returns>Updated Room(s).</returns>
    public Task<List<Room>> UpdateStatuses(Reservation reservation, List<string> roomNumbers);
}
