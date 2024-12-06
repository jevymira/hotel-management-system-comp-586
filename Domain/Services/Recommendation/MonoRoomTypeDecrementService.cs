using Domain.Abstractions.Services;
using Domain.Models;

namespace Domain.Services.Recommendation;

/// <summary>
/// Service to issue room recommendations for only a single room type,
/// not necessarily for the number of rooms specified by the user.
/// </summary>
public class MonoRoomTypeDecrementService : IRoomRecommendationService
{
    /// <summary>
    /// Calculates the number of guests per room, then issues a
    /// recommendation based on that number, decremented.
    /// </summary>
    /// <param name="numGuests">Number of guests specified by the user.</param>
    /// <param name="numRooms">Number of rooms specified by the user.</param>
    /// <param name="availabilities">Room availabilities as determined by the system.</param>
    /// <param name="rooms">Rooms in the system.</param>
    /// <returns>Mono room type recommendation if can be fulfilled by rooms available, null otherwise.</returns>
    public List<RoomOptionDTO> Recommend(int numGuests, int numRooms, 
        RoomAvailabilities availabilities, RoomsByType rooms)
    {
        int guestsPerRoom = DivideAndRoundUp(numGuests, numRooms);

        switch (guestsPerRoom - 1)
        {
            // when clause: to avoid e.g. a recommendation of 20 single rooms
            case 1 when availabilities.Single >= numGuests / 1:
                return new List<RoomOptionDTO> { new RoomOptionDTO {
                    Type = "Single",
                    Quantity = DivideAndRoundUp(numGuests, 1),
                    Price = rooms.Single.First().PricePerNight,
                    ImageUrls = rooms.Single.First().ImageUrls }
                };
            case 2 when availabilities.Double >= numGuests / 2:
                return new List<RoomOptionDTO> { new RoomOptionDTO {
                    Type = "Double",
                    Quantity = DivideAndRoundUp(numGuests, 2),
                    Price = rooms.Double.First().PricePerNight,
                    ImageUrls = rooms.Double.First().ImageUrls }
                };
            case 3 when availabilities.Triple >= numGuests / 3:
                return new List<RoomOptionDTO> { new RoomOptionDTO {
                    Type = "Triple",
                    Quantity = DivideAndRoundUp(numGuests, 3),
                    Price = rooms.Triple.First().PricePerNight,
                    ImageUrls = rooms.Triple.First().ImageUrls }
                };
            // sidelined b/c of double quad for 5 guests/1 room
            /*
            case 4 when availabilities.Quad >= numGuests / 4:
            case 5 when availabilities.Quad >= numGuests / 5:
                return new List<RoomOptionDTO> { new RoomOptionDTO {
                    Type = "Quad",
                    Quantity = DivideAndRoundUp(numGuests, 4),
                    Price = rooms.Quad.First().PricePerNight,
                    ImageUrls = rooms.Quad.First().ImageUrls }
                };
            */
            default: // case 0 and e.g., option of 3 Quads for 9 guests/2 rooms
                return new List<RoomOptionDTO>(); // empty list
        }
    }

    private int DivideAndRoundUp(int divisor, int dividend)
    {
        return (int)Math.Ceiling((double)divisor / (double)dividend);
    }
}
