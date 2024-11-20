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
                return new CheckInService(_roomRepository);
            case "Checked Out":
                return new CheckOutService(_roomRepository);
            case "Due In":
                return new RevertToDueInService(_roomRepository);
            case "Confirmed":
                return new RevertToConfirmedService(_roomRepository);
            default:
                throw new ArgumentException("Unrecognized room status type.");
        }
    }
}
