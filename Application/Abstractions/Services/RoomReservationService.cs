using Application.Entities;

namespace Application.Abstractions.Services;

public abstract class RoomReservationService
{
    private readonly IRoomStatusService _roomsStatusService;

    public RoomReservationService(IRoomStatusService roomsStatusService)
    {
        _roomsStatusService = roomsStatusService;
    }

    public abstract Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers);
}
