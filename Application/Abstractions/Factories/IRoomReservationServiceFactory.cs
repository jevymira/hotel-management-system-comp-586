using Domain.Abstractions.Services;

namespace Application.Abstractions.Factories;

/// <summary>
/// Factory to instantiate the corresponding strategy at runtime.
/// </summary>
public interface IRoomReservationServiceFactory
{
    /// <summary>
    /// Instantiates the strategy corresponding to the reservation status.
    /// </summary>
    /// <param name="status">Reservation status: Checked In, Checked Out, etc.</param>
    /// <returns></returns>
    public IRoomReservationService GetRoomReservationStrategy(string status);
}
