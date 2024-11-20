using Application.Abstractions.Services;
using Application.Helpers.Services;
using Application.Models;
using Domain.Abstractions.Repositories;
using Domain.Entities;

namespace Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRoomRepository _roomRepository;

    public ReservationService(IReservationRepository reservationRepository, IRoomRepository roomRepository)
    {
        _reservationRepository = reservationRepository;
        _roomRepository = roomRepository;
    }

    public async Task<Reservation> AddAsync(PostReservationDTO dto)
    {
        Reservation reservation = new Reservation
        {
            ReservationID = IdGenerator.Get10CharNumericBase10(),
            RoomType = dto.RoomType,
            OrderQuantity = dto.OrderQuantity,
            RoomIDs = new List<string>(),
            // CheckInDate = dto.CheckInDate,
            // CheckOutDate = dto.CheckOutDate,
            NumberOfGuests = dto.NumberOfGuests,
            TotalPrice = dto.TotalPrice,
            BookingStatus = "Confirmed",
            GuestFullName = dto.GuestFullName,
            GuestDateOfBirth = dto.GuestDateOfBirth,
            GuestEmail = dto.GuestEmail,
            GuestPhoneNumber = dto.GuestPhoneNumber,
            UpdatedBy = String.Empty,
        };
        reservation.SetCheckInAndCheckOutDate(dto.CheckInDate, dto.CheckOutDate);
        await _reservationRepository.SaveAsync(reservation);
        return reservation;
    }

    public async Task<Reservation> GetAsync(string id)
    {
        return await _reservationRepository.LoadReservationAsync(id);
    }

    public async Task<List<Reservation>> GetByGuestNameAsync(string name)
    {
        return await _reservationRepository.QueryByNameAsync(name);
    }

    // returns in order of:
    // all due in reservations
    // all checked in reservations
    // checked out reservations of the current date
    // confirmed reservations with a check in date from the current date onward
    public async Task<List<Reservation>> GetForDeskAsync()
    {
        TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        string date = TimeZoneInfo.ConvertTime(DateTime.UtcNow, pacificZone).ToString("yyyy-MM-dd");
        List<Reservation> reservations = new List<Reservation>();

        reservations.AddRange(await _reservationRepository.QueryDueInAsync());
        reservations.AddRange(await _reservationRepository.QueryCheckedInAsync());
        reservations.AddRange(await _reservationRepository.QueryCheckedOutAsync(date));
        reservations.AddRange(await _reservationRepository.QueryConfirmedAsync(date));

        return reservations;
    }

    public async Task<bool> UpdateCheckInOutAsync(string id, CheckInOutDTO dto)
    {
        List<string> roomIDs = new List<string>();
        if (dto.ReservationStatus.Equals("Checked In"))
        {
            // from the room numbers provided, find the room ids
            foreach (string roomNumber in dto.RoomNumbers)
            {
                var room = await _roomRepository.QueryEmptyByRoomNumberAsync(roomNumber);
                if (room == null) { return false; }
                roomIDs.Add(room.RoomID);
            }
            await _reservationRepository.TransactWriteCheckInAsync(id, dto.ReservationStatus, dto.UpdatedBy, roomIDs);
        }
        else // Checked Out, Cancelled
        {
            // from the reservation, find the room IDs
            var reservation = await _reservationRepository.LoadReservationAsync(id);
            roomIDs = reservation.RoomIDs;
            // separate from TransactWriteCheckInAsync
            await _reservationRepository.TransactWriteCheckOutAsync(id, dto.ReservationStatus, dto.UpdatedBy, roomIDs);
        }
        return true;
    }
}
