﻿using Application.Models;
using Domain.Abstractions.Services;

namespace Application.Abstractions.Factories;

public interface IRoomReservationServiceFactory
{
    public IRoomReservationService GetRoomReservationService(string status);
}
