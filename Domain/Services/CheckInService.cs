﻿using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;

namespace Domain.Services;

public class CheckInService : IRoomReservationService
{
    private readonly IRoomsStatusService _roomsStatusService;

    public CheckInService(IRoomsStatusService roomsStatusService)
    {
        _roomsStatusService = roomsStatusService;
    }

    public async Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = await _roomsStatusService.UpdateStatuses(reservation, roomNumbers);

        reservation.CheckIn(rooms); // additionally assigns the rooms passed as parameters

        return rooms;
    }
}
