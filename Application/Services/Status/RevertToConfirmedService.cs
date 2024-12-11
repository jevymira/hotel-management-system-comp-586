using Application.Abstractions.Services;
using Application.Entities;

namespace Application.Services.Status;

public class RevertToConfirmedService : IRoomReservationService
{
    private readonly IRoomStatusService _roomsStatusService;

    public RevertToConfirmedService(IRoomStatusService roomsStatusService)
    {
        _roomsStatusService = roomsStatusService;
    }

    public async Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = await _roomsStatusService.UpdateStatuses(reservation, roomNumbers);

        reservation.MarkConfirmed(); // clears room assignment in reservation

        return rooms;
    }
}
