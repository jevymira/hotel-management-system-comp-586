using Application.Abstractions.Services;
using Application.Entities;

namespace Application.Services.Status;

public class SetCancelledService : RoomReservationService
{
    private readonly IRoomStatusService _roomsStatusService;

    public SetCancelledService(IRoomStatusService roomsStatusService) : base(roomsStatusService)
    {
        _roomsStatusService = roomsStatusService;
    }

    public override async Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = await _roomsStatusService.UpdateStatuses(reservation, roomNumbers);

        reservation.MarkCancelled(); // clears room assignment in reservation

        return rooms;
    }
}
