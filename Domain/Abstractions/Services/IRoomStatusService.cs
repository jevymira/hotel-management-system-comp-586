using Domain.Entities;

namespace Domain.Abstractions.Services;

public interface IRoomStatusService
{
    public Task<List<Room>> UpdateStatuses(Reservation reservation, List<string> roomNumbers);
}
