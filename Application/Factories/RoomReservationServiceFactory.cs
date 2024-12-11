using Application.Abstractions.Factories;
using Application.Abstractions.Repositories;
using Application.Abstractions.Services;
using Application.Services.Status;

namespace Application.Factories;

public class RoomReservationServiceFactory : IRoomReservationServiceFactory
{
    public IRoomRepository _roomRepository;

    public RoomReservationServiceFactory(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }
    public RoomReservationService GetRoomReservationService(string status)
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
