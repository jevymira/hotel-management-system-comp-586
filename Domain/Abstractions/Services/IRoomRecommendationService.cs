using Domain.Models;

namespace Domain.Abstractions.Services;

/// <summary>
/// Common Strategy interface for room recommendation services.
/// </summary>
public interface IRoomRecommendationService
{
    /// <summary>
    /// Generates a recommendations of rooms based on the parameters.
    /// </summary>
    /// <param name="numGuests">Number of guests specified by the user.</param>
    /// <param name="numRooms">Number of rooms specified by the user.</param>
    /// <param name="availabilities">Room availabilities as determined by the system.</param>
    /// <param name="rooms">Rooms in the system.</param>
    /// <returns></returns>
    public List<RoomOptionDTO> Recommend(int numGuests, int numRooms,
        RoomAvailabilities availabilities, RoomsByType rooms);
}
