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
            case "Due In":
            case "Confirmed":
                return new CheckOutService(_roomRepository);
            // TODO: ADD DUE IN + CONFIRMED
            default:
                throw new ArgumentException("Unrecognized room status type.");
        }
    }
}
