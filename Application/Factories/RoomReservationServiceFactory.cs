using Application.Abstractions.Factories;
using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Services;

namespace Application.Factories;

public class RoomReservationServiceFactory : IRoomReservationServiceFactory
{
    public IRoomRepository _roomRepository;

    public RoomReservationServiceFactory(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }
    public IRoomReservationService GetRoomReservationService(string status)
    {
        switch (status)
        {
            case "Checked In":
                return new CheckInService(new OccupyRoomsService(_roomRepository));
            case "Checked Out":
                return new CheckOutService(new EmptyRoomsService(_roomRepository));
            case "Due In":
                return new RevertToDueInService(new EmptyRoomsService(_roomRepository));
            case "Confirmed":
                return new RevertToConfirmedService(new EmptyRoomsService(_roomRepository));
            case "Cancelled":
                return new SetCancelledService(new EmptyRoomsService(_roomRepository));
            default:
                throw new ArgumentException("Unrecognized room status type.");
        }
    }
}
