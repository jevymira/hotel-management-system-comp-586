using Application.Entities;

namespace Application.Abstractions.Services;

public interface IRoomStatusService
{
    public Task<List<Room>> UpdateStatuses(Reservation reservation, List<string> roomNumbers);
}
