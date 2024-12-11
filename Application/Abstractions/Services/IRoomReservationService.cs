using Application.Entities;

namespace Application.Abstractions.Services;

public interface IRoomReservationService
{
    public Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers);
}
