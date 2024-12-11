using Application.Abstractions.Services;
using Application.Entities;

namespace Application.Contexts;

public class RoomReservationServiceContext
{
    private RoomReservationService _strategy;

    public RoomReservationServiceContext(RoomReservationService strategy)
    {
        _strategy = strategy;
    }

    public async Task<List<Room>> RunAssignmentService(Reservation reservation, List<string> roomNumbers)
    {
        return await _strategy.Process(reservation, roomNumbers);
    }
}
