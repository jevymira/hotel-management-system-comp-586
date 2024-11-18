using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;
using Domain.Models;
using System.Reflection;
using System;
using Amazon.DynamoDBv2.DataModel;

namespace LambdaASP.NETCore.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IRoomRepository _roomRepository;

    public ReservationService(IReservationRepository reservationRepository, IRoomRepository roomRepository)
    {
        _reservationRepository = reservationRepository;
        _roomRepository = roomRepository;
    }

    public async Task<Reservation> AddAsync(PostReservationDTO reservationDTO)
    {
        Reservation reservation = new Reservation
        {
            ReservationID = IdGenerator.Get10CharNumericBase10(),
            RoomType = reservationDTO.RoomType,
            OrderQuantity = reservationDTO.OrderQuantity,
            RoomIDs = new List<string>(),
            CheckInDate = reservationDTO.CheckInDate,
            CheckOutDate = reservationDTO.CheckOutDate,
            NumberOfGuests = reservationDTO.NumberOfGuests,
            TotalPrice = reservationDTO.TotalPrice,
            BookingStatus = "Confirmed",
            GuestFullName = reservationDTO.GuestFullName,
            GuestDateOfBirth = reservationDTO.GuestDateOfBirth,
            GuestEmail = reservationDTO.GuestEmail,
            GuestPhoneNumber = reservationDTO.GuestPhoneNumber,
            UpdatedBy = String.Empty,
        };
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

    public async Task UpdateCheckInOutAsync(string id, CheckInOutDTO dto)
    {
        List<string> roomIDs = new List<string>();
        if (dto.ReservationStatus.Equals("Checked In"))
        {
            // from the room numbers provided, find the room ids
            foreach (string roomNumber in dto.RoomNumbers)
            {
                try
                {
                    var room = await _roomRepository.QueryEmptyByRoomNumberAsync(roomNumber);
                    roomIDs.Add(room.RoomID);
                }
                catch (InvalidOperationException)
                {
                    throw new InvalidOperationException("One or more provided room numbers are non-existent or occupied.");
                }
            }
        }
        else if (dto.ReservationStatus.Equals("Checked Out"))
        {
            // from the reservation, find the room IDs
            var reservation = await _reservationRepository.LoadReservationAsync(id);
            roomIDs = reservation.RoomIDs;
        }
        await _reservationRepository.TransactWriteCheckInAsync(id, dto, roomIDs);
    }
}
