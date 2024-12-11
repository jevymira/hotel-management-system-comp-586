using Domain.Abstractions.Services;
using Domain.Entities;

namespace Application.Contexts;

/// <summary>
/// Context class for clients, delegating to IRoomReservationService strategies.
/// </summary>
public class RoomReservationServiceContext
{
    private IRoomReservationService _strategy;

    /// <summary>
    /// Method for swapping in another strategy at runtime.
    /// </summary>
    /// <param name="strategy">Strategy to which to swap.</param>
    public RoomReservationServiceContext(IRoomReservationService strategy)
    {
        _strategy = strategy;
    }

    /// <summary>
    /// Executes, delegating the neccessary responsibilities to the current
    /// concrete strategy implementing the common IRoomReservationService interface.
    /// </summary>
    /// <param name="reservation"></param>
    /// <param name="roomNumbers"></param>
    /// <returns></returns>
    public async Task<List<Room>> ExecuteStrategy(Reservation reservation, List<string> roomNumbers)
    {
        return await _strategy.Process(reservation, roomNumbers);
    }
}
