using Domain.Entities;

namespace Domain.Abstractions.Services;

public interface IRoomsStatusService
{
    public Task<List<Room>> UpdateStatuses(Reservation reservation, List<string> roomNumbers);
}
