using Application.Abstractions.Services;
using Application.Models;

namespace Application.Contexts;

/// <summary>
/// Context class for clients, delegating to IRoomRecommendationService strategies.
/// </summary>
public class RoomRecommendationServiceContext
{
    // abstraction, rather than concrete strategy class
    private IRoomRecommendationService _strategy;

    public RoomRecommendationServiceContext(IRoomRecommendationService strategy)
    {
        _strategy = strategy;
    }

    /// <summary>
    /// Method for swapping in another strategy at runtime.
    /// </summary>
    /// <param name="strategy">Strategy to which to swap.</param>
    public void SetRecommendationStrategy(IRoomRecommendationService strategy)
    {
        _strategy = strategy;
    }

    /// <summary>
    /// Executes, delegating the neccessary responsibilities to the current
    /// concrete strategy implementing the common IRoomRecommendationService interface.
    /// </summary>
    /// <param name="numGuests">Number of guests specified by the user.</param>
    /// <param name="numRooms">Number of rooms specified by the user.</param>
    /// <param name="availabilities">Room availabilities as determined by the system.</param>
    /// <param name="rooms">Rooms in the system.</param>
    /// <returns></returns>
    public List<RoomOptionDTO> ExecuteAlgorithm(
        int numGuests, int numRooms, 
        RoomAvailabilities availabilities, RoomsByType rooms)
    {
        return _strategy.Recommend(numGuests, numRooms, availabilities, rooms);
    }
}
