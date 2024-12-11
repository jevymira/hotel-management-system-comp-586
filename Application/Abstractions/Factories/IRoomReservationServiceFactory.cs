using Application.Abstractions.Services;
using Application.Models;

namespace Application.Abstractions.Factories;

public interface IRoomReservationServiceFactory
{
    public RoomReservationService GetRoomReservationService(string status);
}
