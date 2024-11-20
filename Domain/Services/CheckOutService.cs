using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;

namespace Domain.Services;

public class CheckOutService : IRoomReservationService
{
    private readonly IRoomRepository _roomRepository;

    public CheckOutService(IRoomRepository roomRepository)
    {
        _roomRepository = roomRepository;
    }

    public async Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers, string checkIn, string checkOut, string updatedBy)
    {
        List<Room> rooms = new List<Room>();

        // from the reservation, find the rooms and change their status
        foreach (string roomID in reservation.RoomIDs)
        {
            Room? room = await _roomRepository.LoadRoomAsync(roomID);
            if (room != null)
            {
                rooms.Add(room);
                rooms.Last().Status = "Empty";
                rooms.Last().UpdatedBy = updatedBy;
            }
        }

        reservation.CheckOut();
        reservation.SetDatesForExisting(checkIn, checkOut);
        reservation.UpdatedBy = updatedBy;

        return rooms;
    }
}
