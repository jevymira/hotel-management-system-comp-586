using Application.Abstractions.Services;
using Application.Models;

namespace Application.Services.Recommendation;

/// <summary>
/// Service to issue room recommendations for only a single room type,
/// strictly adhering to the number of rooms specified by the user.
/// </summary>
public class MonoRoomTypeService : IRoomRecommendationService
{
    /// <summary>
    /// Calculates the number of guests per room, then issues a recommendation
    /// based on the number, strictly adhering to the number of rooms specified.
    /// </summary>
    /// <param name="numGuests">Number of guests specified by the user.</param>
    /// <param name="numRooms">Number of rooms specified by the user.</param>
    /// <param name="availabilities">Room availabilities as determined by the system.</param>
    /// <param name="rooms">Rooms in the system.</param>
    /// <returns>Mono room type recommendation if can be fulfilled by rooms available, null otherwise.</returns>
    public List<RoomOptionDTO> Recommend(int numGuests, int numRooms, RoomAvailabilities availabilities, RoomsByType rooms)
    {
        int guestsPerRoom = (int)Math.Ceiling((double)numGuests / (double)numRooms); // rounding up

        switch (guestsPerRoom) // when clause: to not exceed available rooms
        {
            case 1 when numRooms <= availabilities.Single:
                return new List<RoomOptionDTO> { new RoomOptionDTO { Type = "Single", Quantity = numRooms,
                    Price = rooms.Single.First().PricePerNight, ImageUrls = rooms.Single.First().ImageUrls }
                };
            case 2 when numRooms <= availabilities.Double:
                return new List<RoomOptionDTO> { new RoomOptionDTO { Type = "Double", Quantity = numRooms,
                    Price = rooms.Double.First().PricePerNight, ImageUrls = rooms.Double.First().ImageUrls } 
                };
            case 3 when numRooms <= availabilities.Triple:
                return new List<RoomOptionDTO> { new RoomOptionDTO { Type = "Triple", Quantity = numRooms,
                    Price = rooms.Triple.First().PricePerNight, ImageUrls = rooms.Triple.First().ImageUrls } 
                };
            case 4 when numRooms <= availabilities.Quad:
            case 5 when numRooms <= availabilities.Quad:
                return new List<RoomOptionDTO> { new RoomOptionDTO { Type = "Quad", Quantity = numRooms,
                    Price = rooms.Quad.First().PricePerNight, ImageUrls = rooms.Quad.First().ImageUrls } 
                };
            default:
                return new List<RoomOptionDTO>();
        }
    }
}
