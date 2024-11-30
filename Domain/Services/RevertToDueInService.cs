using Domain.Abstractions.Services;
using Domain.Entities;

namespace Domain.Services;

public class RevertToDueInService : IRoomReservationService
{
    private readonly IRoomsStatusService _roomsStatusService;

    public RevertToDueInService(IRoomsStatusService roomsStatusService)
    {
        _roomsStatusService = roomsStatusService;
    }

    public async Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = await _roomsStatusService.UpdateStatuses(reservation, roomNumbers);

        reservation.MarkDueIn(); // clears room assignment in reservation

        return rooms;
    }
}
