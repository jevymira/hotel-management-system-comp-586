using Domain.Abstractions.Services;
using Domain.Entities;

namespace Application.Contexts;

public class RoomReservationServiceContext
{
    private IRoomReservationService _strategy;

    public RoomReservationServiceContext(IRoomReservationService strategy)
    {
        _strategy = strategy;
    }

    public async Task<List<Room>> ExecuteStrategy(Reservation reservation, List<string> roomNumbers)
    {
        return await _strategy.Process(reservation, roomNumbers);
    }
}
