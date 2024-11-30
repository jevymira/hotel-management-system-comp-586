﻿using Application.Models;
using Domain.Abstractions.Services;
using Domain.Entities;

namespace Application.Abstractions.Services;

public interface IReservationService
{
    public Task<Reservation> AddAsync(PostReservationDTO reservationDTO);
    public Task<Reservation> GetAsync(string id);
    public Task<List<ReservationDTO>> GetByGuestNameAsync(string name);
    public Task<List<ReservationDTO>> GetForDeskAsync();
    public Task UpdateStatusAndRoomsAsync(string id, UpdateReservationDTO dto);
    public Task UpdateConfirmedToDueInAsync();
}
