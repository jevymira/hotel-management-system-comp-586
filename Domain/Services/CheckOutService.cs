﻿using Domain.Abstractions.Repositories;
using Domain.Abstractions.Services;
using Domain.Entities;

namespace Domain.Services;

public class CheckOutService : IRoomReservationService
{
    private readonly IRoomsStatusService _roomsStatusService;

    public CheckOutService(IRoomsStatusService roomsStatusService)
    {
        _roomsStatusService = roomsStatusService;
    }

    public async Task<List<Room>> Process(Reservation reservation, List<string> roomNumbers)
    {
        List<Room> rooms = await _roomsStatusService.UpdateStatuses(reservation, roomNumbers);

        reservation.CheckOut(); // clears room assignment in reservation

        return rooms;
    }
}
