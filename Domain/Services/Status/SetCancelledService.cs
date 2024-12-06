using Domain.Abstractions.Services;
using Domain.Entities;

namespace Domain.Services.Status;

public class SetCancelledService : IRoomReservationService
{
    private readonly IRoomStatusService _roomsStatusService;

    public SetCancelledService(IRoomStatusService roomsStatusService)
    {
        _roomsStatusService = roomsStatusService;
    }

    public async Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = await _roomsStatusService.UpdateStatuses(reservation, roomNumbers);

        reservation.MarkCancelled(); // clears room assignment in reservation

        return rooms;
    }
}
