using Domain.Entities;

namespace Domain.Abstractions.Services;

public interface IRoomReservationService
{
    public Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers);
}
