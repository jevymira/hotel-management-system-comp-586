using Domain.Abstractions.Services;
using Domain.Models;

namespace Domain.Services.Recommendation;

/// <summary>
/// Service to issue room recommendations based on a variety of room types,
/// the total number of which not being restricted to the user-provided number.
/// </summary>
public class MultiRoomTypeService : IRoomRecommendationService
{
    private int _numGuests;
    private int _numRooms;

    /// <summary>
    /// Recommends a mix of room types in varying quantities to accomodate the
    /// specified number of guests.
    /// </summary>
    /// <param name="numGuests">Number of guests specified by the user.</param>
    /// <param name="numRooms">Number of rooms specified by the user.</param>
    /// <param name="availabilities">Room availabilities as determined by the system.</param>
    /// <param name="rooms">Rooms in the system.</param>
    /// <returns>Multi-type recommendation if fulfillable by rooms available, null otherwise.</returns>
    public List<RoomOptionDTO> Recommend(int numGuests, int numRooms, RoomAvailabilities availabilities, RoomsByType rooms)
    {
        _numGuests = numGuests;
        _numRooms = numRooms;

        List<RoomOptionDTO> option = new List<RoomOptionDTO>();

        // go down the room types, start with room with the highest capacity
        if (rooms.Quad.Count != 0) // prevent ArgumentNullException
        {
            AddElementIfNotNull(option, GeneratePart("Quad", 4, availabilities.Quad, 
                rooms.Quad.First().PricePerNight, rooms.Quad.First().ImageUrls));
        }
        // proceed to the room type with the second highest capacity
        if (rooms.Triple.Count != 0)
        {
            AddElementIfNotNull(option, GeneratePart("Triple", 3, availabilities.Triple, 
                rooms.Triple.First().PricePerNight, rooms.Triple.First().ImageUrls));
        }
        // second smallest room type
        if (rooms.Double.Count != 0)
        {
            AddElementIfNotNull(option, GeneratePart("Double", 2, availabilities.Double, 
                rooms.Double.First().PricePerNight, rooms.Double.First().ImageUrls));
        }
        // smallest room type for last
        if (rooms.Single.Count != 0)
        {
            AddElementIfNotNull(option, GeneratePart("Single", 1, availabilities.Single, 
                rooms.Single.First().PricePerNight, rooms.Single.First().ImageUrls));
        }

        if (_numGuests == 0) // can accomodate full number of guests specified
        {
            return option;
        }
        else // otherwise, option not returned
        {
            return new List<RoomOptionDTO>(); // empty list
        }
    }

    private RoomOptionDTO? GeneratePart(string name, int capacity, int available, decimal price, List<string> urls)
    {
        int requiredRooms = _numGuests / capacity;
        _numGuests = _numGuests % capacity; // remainder of previous operation
        if (requiredRooms > 0) // prevent appearance of Quantity: 0
        {
            if (requiredRooms > available) // take all available quads
            {
                _numGuests += capacity * (requiredRooms - available); // add remaining guests to remainder
                return new RoomOptionDTO { Type = name, Quantity = available, Price = price, ImageUrls = urls };

            }
            else // take the needed number of quad rooms
            {
                return new RoomOptionDTO { Type = name, Quantity = requiredRooms, Price = price, ImageUrls = urls };
            }
        }
        else
        {
            return null;
        }
    }

    private void AddElementIfNotNull(List<RoomOptionDTO> option, RoomOptionDTO? part)
    {
        if (part != null)
            option.Add(part);
    }
}
