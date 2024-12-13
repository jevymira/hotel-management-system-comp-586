using Application.Abstractions.Factories;
using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Services.Status;

namespace Application.Factories;

/// <summary>
/// Factory to instantiate the corresponding strategy at runtime.
/// </summary>
public class RoomReservationServiceFactory : IRoomReservationServiceFactory
{
    public IRoomRepository _roomRepository;

    public RoomReservationServiceFactory(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    /// <summary>
    /// Instantiates the strategy corresponding to the reservation status.
    /// </summary>
    /// <param name="status">Reservation status: Checked In, Checked Out, etc.</param>
    /// <returns></returns>
    public RoomReservationService CreateRoomReservationStrategy(string status)
    {
        switch (status)
        {
            case "Checked In":
                return new CheckInService(new OccupyRoomsService(_roomRepository));
            case "Checked Out":
                return new CheckOutService(new EmptyRoomsService(_roomRepository));
            case "Due In":
                return new SetDueInService(new EmptyRoomsService(_roomRepository));
            case "Confirmed":
                return new SetConfirmedService(new EmptyRoomsService(_roomRepository));
            case "Cancelled":
                return new SetCancelledService(new EmptyRoomsService(_roomRepository));
            default:
                throw new ArgumentException("Unrecognized room status type.");
        }
    }
}
