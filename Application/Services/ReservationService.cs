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
            // BookingStatus = "Confirmed",
            GuestFullName = dto.GuestFullName,
            GuestDateOfBirth = dto.GuestDateOfBirth,
            GuestEmail = dto.GuestEmail,
            GuestPhoneNumber = dto.GuestPhoneNumber,
            UpdatedBy = String.Empty,
        };

        reservation.SetCheckInAndCheckOutDate(dto.CheckInDate, dto.CheckOutDate);
        reservation.MakeConfirmed();

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
        // coordinate service to load reservation
        var reservation = await _reservationRepository.LoadReservationAsync(id);
        // run-time polymorphism: varying service implementation based on new reservation status
        _roomReservationService = _roomReservationServiceFactory.GetRoomReservationService(dto.ReservationStatus);
        // coordinate the room-reservation service to make changes to the rooms and reservations
        var rooms = await _roomReservationService.Process(reservation, dto.RoomNumbers, dto.UpdatedBy);
        // coordinate repository to persist changes
        await _reservationRepository.TransactWriteRoomReservationAsync(reservation, rooms);
    }
}
