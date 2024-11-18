using Domain.Entities;
using Domain.Models;

namespace Domain.Abstractions.Services;

public interface IReservationService
{
    public Task<Reservation> CreateAsync(PostReservationDTO reservationDTO);
    public Task<Reservation> ReadReservationAsync(string id);
    public Task<List<Reservation>> ReadReservationsByNameAsync(string name);
    public Task<List<Reservation>> ReadReservationsForCurrentDay();
    public Task CheckReservationInOutAsync(string id, CheckInOutDTO dto);
}
