﻿using Domain.Entities;
using Domain.Models;

namespace Domain.Abstractions.Repositories;

public interface IReservationRepository
{
    public Task SaveAsync(Reservation reservation);
    public Task<Reservation> LoadReservationAsync(string id);
    public Task<List<Reservation>> QueryByNameAsync(string name);
    public Task<List<Reservation>> QueryDueInAsync();
    public Task<List<Reservation>> QueryCheckedInAsync();
    public Task<List<Reservation>> QueryCheckedOutAsync(string date);
    public Task<List<Reservation>> QueryConfirmedAsync(string date);
    public Task TransactWriteCheckInAsync(string id, CheckInOutDTO dto, List<string> roomIDs);
    public Task TransactWriteCheckOutAsync(string id, CheckInOutDTO dto, List<string> roomIDs);
}
