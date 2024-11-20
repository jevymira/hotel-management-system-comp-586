using Application.Abstractions.Factories;
using Application.Abstractions.Services;
using Application.Helpers.Services;
using Application.Models;
using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;

namespace Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IRoomReservationServiceFactory _roomReservationServiceFactory;
    private IRoomReservationService? _roomReservationService;

    public ReservationService(
        IReservationRepository reservationRepository, 
        IRoomRepository roomRepository,
        IRoomReservationServiceFactory roomReservationServiceFactory)
    {
        _reservationRepository = reservationRepository;
        _roomRepository = roomRepository;
        _roomReservationServiceFactory = roomReservationServiceFactory;
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

    public async Task UpdateStatusAndRoomsAsync(string id, CheckInOutDTO dto)
    {
        var reservation = await _reservationRepository.LoadReservationAsync(id);
        _roomReservationService = _roomReservationServiceFactory.GetRoomReservationService(dto.ReservationStatus);
        var rooms = await _roomReservationService.Process(reservation, dto.RoomNumbers, dto.UpdatedBy);
        await _reservationRepository.TransactWriteRoomReservationAsync(reservation, rooms);

        /*
        List<Room> rooms = new List<Room>();
        if (dto.ReservationStatus.Equals("Checked In"))
        {
            // from the room numbers provided, find the rooms
            foreach (string roomNumber in dto.RoomNumbers)
            {
                var room = await _roomRepository.QueryEmptyByRoomNumberAsync(roomNumber);
                if (room == null) { return false; }
                rooms.Add(room);
            }
            // TODO: unify with other as pure write
            await _reservationRepository.TransactWriteCheckInAsync(id, dto.ReservationStatus, dto.UpdatedBy, roomIDs);
        }
        else // Checked Out, Cancelled
        {
            // from the reservation, find the rooms
            // TODO: retain this line for both
            var reservation = await _reservationRepository.LoadReservationAsync(id);

            foreach (string roomID in reservation.RoomIDs)
            {
                rooms.Add(await _roomRepository.LoadRoomAsync(roomID));
            }
            // separate from TransactWriteCheckInAsync
            // TODO: unify with other as pure write
            await _reservationRepository.TransactWriteCheckOutAsync(id, dto.ReservationStatus, dto.UpdatedBy, roomIDs);
        }
        return true;
        */
    }
}
