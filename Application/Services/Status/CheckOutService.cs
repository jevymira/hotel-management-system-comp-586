using Application.Abstractions.Services;
using Application.Entities;

namespace Application.Services.Status;

public class CheckOutService : IRoomReservationService
{
    private readonly IRoomStatusService _roomsStatusService;

    public CheckOutService(IRoomStatusService roomsStatusService)
    {
        _roomsStatusService = roomsStatusService;
    }

    public async Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = await _roomsStatusService.UpdateStatuses(reservation, roomNumbers);

        reservation.CheckOut(); // clears room assignment in reservation

        return rooms;
    }
}
