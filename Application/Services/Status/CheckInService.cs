using Application.Abstractions.Services;
using Application.Entities;

namespace Application.Services.Status;

public class CheckInService : RoomReservationService
{
    private readonly IRoomStatusService _statusService;

    public CheckInService(IRoomStatusService roomsStatusService) : base(roomsStatusService)
    {
        _statusService = roomsStatusService;
    }

    public override async Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = await _statusService.UpdateStatuses(reservation, roomNumbers);

        reservation.CheckIn(rooms); // additionally assigns the rooms passed as parameters

        return rooms;
    }
}
