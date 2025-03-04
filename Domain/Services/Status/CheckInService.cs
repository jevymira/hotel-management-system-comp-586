﻿using Domain.Abstractions.Services;
using Domain.Entities;

namespace Domain.Services.Status;

/// <summary>
/// Concrete Strategy for a service that handles checking in a reservation
/// and making its room assignments.
/// </summary>
/// <remarks>
/// The Refined Abstraction in a Bridge pattern with Implementor IRoomStatusService.
/// </remarks>
public class CheckInService : RoomReservationService
{
    private readonly IRoomStatusService _roomsStatusService;

    public CheckInService(IRoomStatusService roomsStatusService) : base(roomsStatusService)
    {
        _roomsStatusService = roomsStatusService;
    }

    /// <summary>
    /// Checks in the reservation and assigns rooms which correspond to
    /// the provided roomNumbers, together, to ensure their consistency.
    /// </summary>
    /// <param name="reservation">Reservation to be updated.</param>
    /// <param name="roomNumbers">Room Numbers of rooms to be updated.</param>
    /// <returns>Updated Room(s).</returns>
    public override async Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = await _roomsStatusService.UpdateStatuses(reservation, roomNumbers);

        reservation.CheckIn(rooms); // assigns the rooms passed as parameters

        return rooms;
    }
}
